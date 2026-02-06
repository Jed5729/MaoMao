using MaoMao.API.Models;
using MaoMao.API.Services.Contract;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
using TwoFactorAuthNet;
using YamlDotNet.Core.Tokens;

namespace MaoMao.API.Services;

public class UserService : IUserService
{
	private readonly IMongoCollection<User> _users;
	private ITokenService _tokens;
	private IHashService _hasher;
	private IConfiguration _config;

	public UserService(IMongoDatabase db, IConfiguration config, ITokenService tokenService, IHashService hashService)
	{
		_users = db.GetCollection<User>(config.GetValue<string>("Mongo:UserCollection"));
		
		// TTL to autodelete users with deletion pending after x days configured in appsettings.json
		var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.DeletionRequestedTime);
		var indexOptions = new CreateIndexOptions
		{
			ExpireAfter = TimeSpan.FromDays(config.GetValue<int>("Mongo:DeleteUserPendingDays"))
		};
		var indexModel = new CreateIndexModel<User>(indexKeys, indexOptions);
		_users.Indexes.CreateOne(indexModel);

		// Enforce users unique
		_users.Indexes.CreateOne(new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(u => u.Username), new CreateIndexOptions<User> { Unique = true }));

		_tokens = tokenService;
		_hasher = hashService;
		_config = config;
	}

	private async Task<User> UpdateUserAsync(User user, CancellationToken ct = default)
	{
		user.LastUpdatedDate = DateTime.UtcNow;

		var filter = Builders<User>.Filter.Eq(x => x.UserId, user.UserId);

		var options = new ReplaceOptions { IsUpsert = true };

		await _users.ReplaceOneAsync(filter, user, options, ct);

		return user;
	}

	public async Task<User> FindUserByUsernameAsync(string username, CancellationToken ct = default)
	{
		var filter = Builders<User>.Filter.Eq(x => x.Username, username);
		var result = await _users.FindAsync(filter: filter, cancellationToken: ct);
		return await result.FirstOrDefaultAsync(ct);
	}

	public async Task<User> FindUserByIdAsync(string userId, CancellationToken ct = default)
	{
		var filter = Builders<User>.Filter.Eq(x => x.UserId, userId);
		var result = await _users.FindAsync(filter: filter, cancellationToken: ct);
		return await result.FirstOrDefaultAsync(ct);
	}

	private async Task<bool> IsEmailAlreadyInUse(string email)
	{
		return await _users.Find(x => x.Email == email).AnyAsync();
	}

	public async Task<bool> VerifyEmailAsync(string userId)
	{
		var user = await FindUserByIdAsync(userId);
		if (user.EmailVerified) return false;
		user.EmailVerified = true;
		await UpdateUserAsync(user);
		return true;
	}

	private async Task<bool> IsUsernameAlreadyInUse(string username)
	{
		return await _users.Find(x => x.Username == username).AnyAsync();
	}

	private async Task<(bool Success, List<string> Errors)> GetLoginResult(string username, string preHash)
	{
		preHash = preHash.ToLower();
		var errors = new List<string>();
		var res = await FindUserByUsernameAsync(username);

		if (res == default)
		{
			errors.Add("No account was found under this username.");
			return (false, errors);
		}

		if (await _hasher.VerifyHash(res.PasswordHash, preHash))
		{
			return (true, errors);
		}
		else
		{
			errors.Add("Incorrect password.");
		}

		return (false, errors);
	}

	private User GenerateNewUser(string username, string email, string passwordSecureHash)
	{
		return new User
		{
			Username = username,
			Email = email,
			EmailVerified = false,
			CreatedDate = DateTime.UtcNow,
			LastUpdatedDate = DateTime.UtcNow,
			PasswordHash = passwordSecureHash
		};
	}

	public async Task<(bool Success, string AccessToken, RefreshToken? RefreshToken, List<string> Errors)> RegisterUserAsync(string username, string email, string preHash, string deviceName, string ipAddress)
	{
		var errors = new List<string>();
		preHash = preHash.ToLower();

		if (await IsEmailAlreadyInUse(email))
		{
			errors.Add("Email already in use.");
			return (false, "", null, errors);
		}

		if (await IsUsernameAlreadyInUse(username))
		{
			errors.Add("Username already in use.");
			return (false, "", null, errors);
		}

		var secureHash = await _hasher.HashPassword(preHash);

		User user = await UpdateUserAsync(GenerateNewUser(username, email, secureHash));

		var refreshToken = await _tokens.GenerateRefreshTokenAsync(user, deviceName, ipAddress);
		
		if (refreshToken is null)
		{
			errors.Add("Error generating refreshToken");
			return (false, "", null, errors);
		}

		var accessToken = _tokens.GenerateAccessToken(user, refreshToken.Id);

		return (true, accessToken, refreshToken, errors);
	}

	public async Task<(bool Success, string AccessToken, RefreshToken? RefreshToken, List<string> Errors)> LoginAsync(string username, string preHash, string deviceName, string ipAddress)
	{
		var loginResult = await GetLoginResult(username, preHash);
		var accessToken = "";
		var refreshToken = default(RefreshToken);
		if (loginResult.Success)
		{
			var user = await FindUserByUsernameAsync(username);
			//refreshToken = await _tokens.GenerateRefreshTokenAsync(user.UserId, deviceName, ipAddress);
			//if (refreshToken is null || refreshToken == default)
			//{
			//	loginResult.Errors.Add("RefreshToken object null or default");
			//	return (false, accessToken, null, loginResult.Errors);
			//}
			//accessToken = _tokens.GenerateAccessToken(user, refreshToken.Id);
			var tokensResult = await GenerateLoginTokensAsync(user, deviceName, ipAddress);

			loginResult.Success = tokensResult.Success;
			loginResult.Errors.Concat(tokensResult.Errors);
			accessToken = tokensResult.AccessToken;
			refreshToken = tokensResult.RefreshToken;
		}
		return (loginResult.Success, accessToken, refreshToken, loginResult.Errors);
	}

	public async Task<(bool Success, RefreshToken? RefreshToken, string AccessToken, List<string> Errors)> GenerateLoginTokensAsync(User user, string deviceName = "Unknown", string ipAddress = "unknown")
	{
		var errors = new List<string>();
		if (user is null)
		{
			errors.Add("User is null.");
			return (false, null, string.Empty, errors);
		}
	    if (deviceName == "Unknown")
			errors.Add("No device name was provided.");
		if (ipAddress == "unknown")
			errors.Add("No IP address was provided.");

		var refreshToken = await _tokens.GenerateRefreshTokenAsync(user, deviceName, ipAddress);
		if (refreshToken is null || refreshToken == default)
		{
			errors.Add("RefreshToken object null or default");
			return (false, null, string.Empty, errors);
		}
		var accessToken = _tokens.GenerateAccessToken(user, refreshToken.Id);
		return (true, refreshToken, accessToken, errors);
	}

	public async Task<(bool Success, List<string> Errors)> FlagUserForDeletionAsync(string userId) => await FlagUserForDeletionAsync(await FindUserByIdAsync(userId));

	private async Task<(bool Success, List<string> Errors)> FlagUserForDeletionAsync(User user)
	{
		var errors = new List<string>();
		if (user == default)
		{
			errors.Add("User not found.");
			return (false, errors);
		}
		if (user.IsDeletionRequested)
		{
			errors.Add("Account already flagged for deletion.");
			return (false, errors);
		}
		
		user.IsDeletionRequested = true;
		user.DeletionRequestedTime = DateTime.UtcNow;

		await UpdateUserAsync(user);
		return (true, errors);
	}

	public async Task<(bool Success, string Secret, string QR, List<string> Errors)> SetupTwoFactor(User user)
	{
		var errors = new List<string>();
		var secret = "";
		var qr = "";

		if (user == default)
		{
			errors.Add("User not found.");
			return (false, secret, qr, errors);
		}
		if (user.TwoFactorEnabled)
		{
			errors.Add("Two factor is already enabled.");
			return (false, secret, qr, errors);
		}

		try
		{
			var issuer = _config.GetValue<string>("2FA:Issuer");
			var tfa = new TwoFactorAuth(issuer);
			secret = tfa.CreateSecret(160); // 160 bits recommended (see RFC 4226 - Algorithm Requirements) (according to the gh readme for this library lol)
			qr = tfa.GetQrCodeImageAsDataUri(user.Email, secret); // Data uri schema for image, which contains uri like so:
																  // otpauth://totp/{label}?secret={secret}&issuer={issuer}&period={period}&algorithm={algorithm}&digits={digits}

		}
		catch (Exception ex)
		{
			errors.Add($"Exception in generating 2FA: {ex.Message}");
			return (false, string.Empty, string.Empty, errors);
		}

		user.TwoFactorEnabled = true;
		user.TwoFactorSecret = EncryptTotpSecret(secret, user.UserId);

		await UpdateUserAsync(user);

		return (true, secret, qr, errors);
	}

	public bool VerifyTwoFactorCode(string code, User user)
	{
		if (!user.TwoFactorEnabled || string.IsNullOrWhiteSpace(user.TwoFactorSecret)) return false;

		var secret = DecryptTotpSecret(user.TwoFactorSecret, user.UserId);
		var issuer = _config.GetValue<string>("2FA:Issuer");
		var tfa = new TwoFactorAuth(issuer);
		return tfa.VerifyCode(secret, code);
	}

	private byte[] GetUserBasedEncryptionKey(string userId) => SHA256.HashData(Encoding.UTF8.GetBytes($"UBEC-{_config.GetValue<string>("2FA:EncryptionSecret")}:{userId}"));

	private string EncryptTotpSecret(string secret, string userId)
	{
		var userKey = GetUserBasedEncryptionKey(userId);

		var nonce = RandomNumberGenerator.GetBytes(12);
		var plaintext = Encoding.UTF8.GetBytes(secret);
		var ciphertext = new byte[plaintext.Length];
		var tag = new byte[16];

		using (var aes = new AesGcm(userKey, 16))
		{
			aes.Encrypt(nonce, plaintext, ciphertext, tag);
		}

		var combined = new byte[nonce.Length + tag.Length + ciphertext.Length];
		Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
		Buffer.BlockCopy(tag, 0, combined, nonce.Length, tag.Length);
		Buffer.BlockCopy(ciphertext, 0, combined, nonce.Length + tag.Length, ciphertext.Length);

		return Convert.ToBase64String(combined);
	}

	private string DecryptTotpSecret(string encrypted, string userId)
	{
		var key = GetUserBasedEncryptionKey(userId);
		var data = Convert.FromBase64String(encrypted);

		var nonce = data[..12];
		var tag = data[12..28];
		var ciphertext = data[28..];
		var plaintext = new byte[ciphertext.Length];

		using (var aes = new AesGcm(key, 16))
		{
			aes.Decrypt(nonce, ciphertext, tag, plaintext);
		}

		return Encoding.UTF8.GetString(plaintext);
	}
}

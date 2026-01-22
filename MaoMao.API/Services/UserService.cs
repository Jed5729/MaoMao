using MaoMao.API.Models;
using MaoMao.API.Services.Contract;
using MongoDB.Driver;

namespace MaoMao.API.Services;

public class UserService : IUserService
{
	private readonly IMongoCollection<User> _users;
	private ITokenService _tokens;
	private IHashService _hasher;

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

		var refreshToken = await _tokens.GenerateRefreshTokenAsync(user.UserId, deviceName, ipAddress);
		
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
			refreshToken = await _tokens.GenerateRefreshTokenAsync(user.UserId, deviceName, ipAddress);
			if (refreshToken is null || refreshToken == default)
			{
				loginResult.Errors.Add("RefreshToken object null or default");
				return (false, accessToken, null, loginResult.Errors);
			}
			accessToken = _tokens.GenerateAccessToken(user, refreshToken.Id);
		}
		return (loginResult.Success, accessToken, refreshToken, loginResult.Errors);
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
			errors.Add("Account already flagged for deletion");
			return (false, errors);
		}
		
		user.IsDeletionRequested = true;
		user.DeletionRequestedTime = DateTime.UtcNow;

		await UpdateUserAsync(user);
		return (true, errors);
	}
}

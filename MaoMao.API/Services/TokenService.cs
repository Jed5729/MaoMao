using FastEndpoints.Security;
using MaoMao.API.DTO.User;
using MaoMao.API.Models;
using MaoMao.API.Services.Contract;
using MaoMao.Shared.DTO.User;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MaoMao.API.Services;

public class TokenService : ITokenService
{
	private readonly IMongoCollection<RefreshToken> _tokens;
	private readonly IConfiguration _config;

	public TokenService(IConfiguration config, IMongoDatabase db)
	{
		_config = config;
		_tokens = db.GetCollection<RefreshToken>(config.GetValue<string>("Mongo:RefreshTokenCollection")!);

		// TTL - Auto cleanup expired tokens
		var indexKeys = Builders<RefreshToken>.IndexKeys.Ascending(tk => tk.ExpiresAt);
		var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero };
		var indexModel = new CreateIndexModel<RefreshToken>(indexKeys, indexOptions);
		_tokens.Indexes.CreateOne(indexModel);
	}

	private static string GenerateSecureToken()
	{
		var bytes = RandomNumberGenerator.GetBytes(64);
		return Convert.ToBase64String(bytes);
	}

	public async Task<RefreshToken> GenerateRefreshTokenAsync(User user, string deviceName, string ip)
	{
		var token = new RefreshToken
		{
			Token = GenerateSecureToken(),
			UserId = user.UserId,
			DeviceName = deviceName,
			IpAddress = ip,
			ExpiresAt = DateTime.UtcNow.AddDays(_config.GetValue<int>("JWT:RefreshTokenDurationDays"))
		};

		if (user is not null)
		{
			if (!user.KnownIPs.Contains(ip))
				user.KnownIPs.Add(ip);
		}

		await _tokens.InsertOneAsync(token);

		return token;
	}

	public async Task<RefreshToken?> GetByTokenAsync(string token)
	{
		return await _tokens.Find(tk => tk.Token == token).FirstOrDefaultAsync();
	}

	public async Task<RefreshToken?> GetByIdAsync(string id)
	{
		return await _tokens.Find(tk => tk.Id == id).FirstOrDefaultAsync();
	}

	public async Task RevokeTokenAsync(string id)
	{
		await _tokens.DeleteOneAsync(tk => tk.Id == id);
	}

	public async Task<IEnumerable<Session>> GetActiveSessionsAsync(string userId, string currentRefreshTokenId)
	{
		var sessions = await _tokens.Find(tk => tk.UserId == userId).ToListAsync();

		return sessions.Select(tk => new Session
		{
			Id = tk.Id,
			DeviceName = tk.DeviceName,
			IpAddress = tk.IpAddress,
			ExpiresAt = tk.ExpiresAt,
			CreatedAt = tk.CreatedAt,
			IsCurrentSession = (tk.Id == currentRefreshTokenId)
		});
	}

	public string GenerateAccessToken(User user, string sessionRefreshTokenId)
	{
		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, user.UserId),
			new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
			new Claim(JwtRegisteredClaimNames.Email, user.Email),
			new Claim(JwtRegisteredClaimNames.Sid, sessionRefreshTokenId),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new Claim(JwtRegisteredClaimNames.Typ, "access"),
		};

		return JwtBearer.CreateToken(o =>
		{
			o.SigningKey = _config["JWT:SecretKey"]!;
			o.ExpireAt = DateTime.UtcNow.AddMinutes(_config.GetValue<int>("JWT:ExpireInMinutes"));
			o.Issuer = _config["JWT:Issuer"]!;
			o.Audience = _config["JWT:Audience"]!;
			o.User.Claims.AddRange(claims);
		});
	}

	public string GenerateTwoFactorPendingToken(User user)
	{
		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, user.UserId),
			new Claim(JwtRegisteredClaimNames.Typ, "2fa_pending"),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		return JwtBearer.CreateToken(o =>
		{
			o.SigningKey = _config["JWT:SecretKey"]!;
			o.ExpireAt = DateTime.UtcNow.AddMinutes(_config.GetValue<int>("JWT:2FAPendingTokenMinutes"));
			o.Issuer = _config["JWT:Issuer"]!;
			o.Audience = _config["JWT:Audience"]!;
			o.User.Claims.AddRange(claims);
		});
	}
}

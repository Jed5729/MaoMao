using MaoMao.API.DTO.User;
using MaoMao.API.Models;
using MaoMao.Shared.DTO.User;

namespace MaoMao.API.Services.Contract;
public interface ITokenService
{
	string GenerateAccessToken(User user, string sessionRefreshTokenId);
	Task<RefreshToken> GenerateRefreshTokenAsync(User user, string deviceName, string ip);
	public string GenerateTwoFactorPendingToken(User user);
	Task<IEnumerable<Session>> GetActiveSessionsAsync(string userId, string currentRefreshTokenId);
	Task RevokeTokenAsync(string id);
	Task<RefreshToken?> GetByTokenAsync(string token);
	Task<RefreshToken?> GetByIdAsync(string id);
}
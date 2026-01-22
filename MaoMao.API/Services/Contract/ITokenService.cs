using MaoMao.API.DTO.User;
using MaoMao.API.Models;

namespace MaoMao.API.Services.Contract;
public interface ITokenService
{
	string GenerateAccessToken(User user, string sessionRefreshTokenId);
	Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string deviceName, string ip);
	Task<IEnumerable<Session>> GetActiveSessionsAsync(string userId, string currentRefreshTokenId);
	Task RevokeTokenAsync(string id);
	Task<RefreshToken?> GetByTokenAsync(string token);
	Task<RefreshToken?> GetByIdAsync(string id);
}
using MaoMao.API.Models;

namespace MaoMao.API.Services.Contract;

public interface IUserService
{
	Task<(bool Success, string AccessToken, RefreshToken? RefreshToken, List<string> Errors)> RegisterUserAsync(string username, string email, string preHash, string deviceName, string ipAddress);
	Task<(bool Success, string AccessToken, RefreshToken? RefreshToken, List<string> Errors)> LoginAsync(string username, string preHash, string deviceName, string ipAddress);
	Task<(bool Success, List<string> Errors)> FlagUserForDeletionAsync(string userId);
	Task<User> FindUserByIdAsync(string userId, CancellationToken ct = default);
	Task<bool> VerifyEmailAsync(string userId);
	Task<User> FindUserByUsernameAsync(string username, CancellationToken ct = default);
}
namespace MaoMao.API.Services.Contract;

public interface IHashService
{
	Task<string> HashPassword(string preHash);
	Task<bool> VerifyHash(string storedHash, string preHash);
}
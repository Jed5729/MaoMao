using Isopoh.Cryptography.Argon2;
using MaoMao.API.Services.Contract;

namespace MaoMao.API.Services;

public class HashService : IHashService
{
	public async Task<string> HashPassword(string preHash) => await Task.Run(() => Argon2.Hash(preHash));

	public async Task<bool> VerifyHash(string storedHash, string preHash) => await Task.Run(() => Argon2.Verify(storedHash, preHash));
}

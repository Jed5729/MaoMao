using MaoMao.API.Models;

namespace MaoMao.API.Services.Contract;

public interface IEmailService
{
	Task SendVerificationEmailAsync(string recipient, User userForPersonalization);
	Task<bool> VerifyCode(int code, string userId);
}
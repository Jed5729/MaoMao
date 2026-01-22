using MaoMao.API.Models;
using MaoMao.API.Services.Contract;
using MongoDB.Driver;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

namespace MaoMao.API.Services;

public class EmailService : IEmailService
{
    private readonly IMongoCollection<VerificationCode> _verificationCodes;
    private readonly IConfiguration _config;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpDisplayUser;
    private readonly string _smtpPass;
    private readonly int _codeValidMinutes;

	public EmailService(IConfiguration config, IMongoDatabase db)
	{
		_config = config;

		_smtpHost = _config.GetValue<string>("Email:Host")!;
		_smtpPort = _config.GetValue<int>("Email:Port");
		_smtpUser = _config.GetValue<string>("Email:EmailAddress")!;
		_smtpDisplayUser = _config.GetValue<string>("Email:DisplayName")!;
		_smtpPass = _config.GetValue<string>("Email:AppPassword")!;
        _codeValidMinutes = _config.GetValue<int>("Email:CodeValidMinutes")!;

        _verificationCodes = db.GetCollection<VerificationCode>(config.GetValue<string>("Mongo:VerificationCodeCollection"));
        _verificationCodes.Indexes.CreateOne( // Enforce unique UserId associated with the verification code.
            new CreateIndexModel<VerificationCode>(
                Builders<VerificationCode>.IndexKeys.Ascending(v => v.UserId),
                new CreateIndexOptions { Unique = true }
            )
        );
        _verificationCodes.Indexes.CreateOne( // TTL automatic expiration of codes in the collection 
            new CreateIndexModel<VerificationCode>(
                Builders<VerificationCode>.IndexKeys.Ascending(v => v.TimeIssued),
                new CreateIndexOptions { ExpireAfter = TimeSpan.FromMinutes(_codeValidMinutes) }
            )
        );
	}

	public async Task SendVerificationEmailAsync(string recipient, User user)
	{
		var message = new MailMessage();
		message.From = new MailAddress(_smtpUser, _smtpDisplayUser);
		message.To.Add(recipient);
		message.Subject = "Verify your Email";
		message.IsBodyHtml = true;
		message.Body =
			$"""
            <!doctype html>
            <html>
              <body style="font-family: Arial, sans-serif; background: #f9f9f9; margin: 0; padding: 20px;">
                <table width="100%" cellpadding="0" cellspacing="0" role="presentation">
                  <tr>
                    <td align="center">
                      <table width="480" cellpadding="0" cellspacing="0" role="presentation" style="background:#ffffff; padding:20px; border-radius:8px;">
                        <tr>
                          <td align="center" colspan="2">
                            <h1 style="color:#333333; font-size:20px; margin:0 0 12px;">Email Verification</h1>
                            <p style="margin:0 0 12px; color:#666666; font-size:14px;">
                              Hey there <strong>{user.Username}</strong>! 
                              Thank you for using <strong>MaoMao</strong>.<br>Use the verification code below to confirm your email:
                            </p>
                          </td>
                        </tr>
                        <tr>
                          <td align="right">
                            <img src="https://i.imgur.com/D8outsY.png" alt="MaoMao Logo" width="120" style="border-radius:6px; display:block; margin-right: 10px" />
                          </td>
                          <td align="left">
                            <div style="display:inline-block; padding:12px 20px; background:#f0f6ff; color:#2d89ef; font-weight:700; font-size:24px; letter-spacing:3px; border-radius:6px;">
                              {await GenerateNewVerificationCode(user.UserId)}
                            </div>
                          </td>
                        </tr>
                        <tr>
                          <td align="center" colspan="2">
                            <p style="margin:12px 0 0; color:#666666; font-size:13px;">
                              This code will expire in <strong>{_codeValidMinutes} minutes</strong> from when this was sent.
                            </p>
                            <p style="margin:12px 0 0; color:#aaaaaa; font-size:12px;">
                              If you didn't request this, you can ignore this message.
                            </p>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                </table>
              </body>
            </html>
            """;

		using var client = new SmtpClient(_smtpHost, _smtpPort)
		{
			Credentials = new NetworkCredential(_smtpUser, _smtpPass),
			EnableSsl = true
		};

		await client.SendMailAsync(message);
	}

	private async Task<int> GenerateNewVerificationCode(string userId)
	{
		var code = RandomNumberGenerator.GetInt32(100000, 1000000);

        var vCode = await AddVerificationCodeToDb(code, userId);

        if (!vCode.Success) return -1;

        return vCode.VerificationCode.Code;
	}

    private async Task<(bool Success, VerificationCode VerificationCode)> AddVerificationCodeToDb(int code, string userId)
    {
        var vCode = new VerificationCode()
        {
            UserId = userId,
            Code = code,
            TimeIssued = DateTime.UtcNow
        };

        var success = true;

        try
        {
            var first = await _verificationCodes.Find(v => v.UserId == userId).FirstOrDefaultAsync();

			if (first is not null)
			{
                await _verificationCodes.DeleteOneAsync(x => x.Id == first.Id);
			}

			await _verificationCodes.InsertOneAsync(vCode);
		}
        catch
        {
            success = false;
        }

        return (success, vCode);
    }

    public async Task<bool> VerifyCode(int code, string userId)
    {
        var search = await _verificationCodes.FindAsync(v => v.UserId == userId);
        var first = await search.FirstOrDefaultAsync();

        if (first is not null)
        {
            if(first.Code == code)
            {
                await _verificationCodes.DeleteOneAsync(x => x.Id == first.Id);
                return true;
            }
        }

        return false;
    }
}

using MaoMao.API.DTO.User;
using MaoMao.Shared.DTO.User;

namespace MaoMao.API.Endpoints.User;

public class VerifyEmailEndpoint(IEmailService emails, IUserService users) : Endpoint<VerifyEmailRequest>
{
	public override void Configure()
	{
		Post("/user/verifyemail");
		AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
		Policies("DefaultUser");
	}

	public override async Task HandleAsync(VerifyEmailRequest req, CancellationToken ct)
	{
		var user = await users.FindUserByIdAsync(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "");

		if(user.EmailVerified)
		{
			await Send.UnauthorizedAsync();
			return;
		}

		if (await emails.VerifyCode(req.Code, user.UserId))
		{
			await users.VerifyEmailAsync(user.UserId);
			await Send.OkAsync();
			return;
		}

		await Send.UnauthorizedAsync();
	}
}

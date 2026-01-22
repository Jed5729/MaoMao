using MaoMao.API.Services.Contract;

namespace MaoMao.API.Endpoints.User;

public class SendVerificationEndpoint(IUserService users, IEmailService emails) : EndpointWithoutRequest
{
	public override void Configure()
	{
		Get("/user/sendverification");
		AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var user = await users.FindUserByIdAsync(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "");

		if (user is null || user.EmailVerified)
		{
			await Send.UnauthorizedAsync(ct);
			return;
		}

		await emails.SendVerificationEmailAsync(user.Email, user);

		await Send.OkAsync(ct);
	}
}

using MaoMao.API.Services.Contract;

namespace MaoMao.API.Endpoints.User;

[Throttle(hitLimit: 3, durationSeconds: 600)] // 3 requests per 10 minutes- should help prevent over-usage on email client.
public class SendVerificationEndpoint(IUserService users, IEmailService emails) : EndpointWithoutRequest
{
	public override void Configure()
	{
		Get("/user/sendverification");
		AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
		Policies("DefaultUser");
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

using MaoMao.Shared.DTO.User;

namespace MaoMao.API.Endpoints.User;

public class EnableTwoFactorEndpoint(IUserService users) : EndpointWithoutRequest<EnableTwoFactorResponse>
{
	public override void Configure()
	{
		Post("/user/enabletwofactor");
		AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
		Policies("DefaultUser");
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var user = await users.FindUserByIdAsync(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "");

		var result = await users.SetupTwoFactor(user);

		if (!result.Success)
		{
			foreach (var e in result.Errors)
				AddError(e);
			await Send.ErrorsAsync(cancellation: ct);
			return;
		}

		await Send.OkAsync(new EnableTwoFactorResponse()
		{
			Secret = result.Secret,
			QRUri = result.QR
		});
	}
}

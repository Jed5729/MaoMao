using MaoMao.API.Models;
using MaoMao.Shared.DTO.Auth;
using MaoMao.Shared.DTO.User;
using System.Diagnostics.Eventing.Reader;

namespace MaoMao.API.Endpoints.Auth;

public class VerifyTwoFactorEndpoint(IUserService users) : Endpoint<VerifyTwoFactorRequest, AuthResponse>
{
	public override void Configure()
	{
		Post("/auth/verifytwofactor");
		AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
		Policies("TwoFactorPending");
	}

	public override async Task HandleAsync(VerifyTwoFactorRequest req, CancellationToken ct)
	{
		//var onPendingToken = User.FindFirstValue(JwtRegisteredClaimNames.Typ) == "2fa_pending";
		var user = await users.FindUserByIdAsync(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "");

		var success = users.VerifyTwoFactorCode(req.Code, user);

		if (!success)
		{
			await Send.UnauthorizedAsync();
		}

		//if (onPendingToken)
		//{
		var result = await users.GenerateLoginTokensAsync(user, req.Device, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

		if (!result.Success || result.RefreshToken is null)
		{
			foreach (var e in result.Errors)
				AddError(e);
			await Send.ErrorsAsync(cancellation: ct);
			return;
		}

		await Send.OkAsync(new AuthResponse()
		{
			AccessToken = result.AccessToken,
			RefreshToken = result.RefreshToken.Token,
			RefreshTokenId = result.RefreshToken.Id
		});
		//}
		//else
		//{
		//	await Send.UnauthorizedAsync();
		//	return;
		//}
	}
}

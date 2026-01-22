using MaoMao.API.DTO.Auth;
using MaoMao.API.Services;
using MaoMao.API.Services.Contract;

namespace MaoMao.API.Endpoints.Auth;

[Throttle(hitLimit: 5, durationSeconds: 300)] // 5 attempts per 5 minutes
public class RegisterUserEndpoint(IUserService _users) : Endpoint<RegisterUserRequest, AuthResponse>
{
	public override void Configure()
	{
		Post("/auth/register");
		AllowAnonymous();
	}

	public override async Task HandleAsync(RegisterUserRequest req, CancellationToken ct)
	{
		var result = await _users.RegisterUserAsync(req.Username, req.Email, req.PreHashedPassword, req.DeviceName, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

		if (!result.Success)
		{
			foreach (var e in result.Errors)
				AddError(e);
			await Send.ErrorsAsync(cancellation: ct);
			return;
		}

		await Send.OkAsync(new AuthResponse()
		{
			AccessToken = result.AccessToken,
			RefreshToken = result.RefreshToken!.Token,
			RefreshTokenId = result.RefreshToken.Id
		}, ct);
	}
}

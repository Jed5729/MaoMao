using MaoMao.Shared.DTO.Auth;

namespace MaoMao.API.Endpoints.Auth;

[Throttle(hitLimit: 10, durationSeconds: 300)] // 10 attempts per 5 minutes
public class LoginEndpoint(IUserService _users, ITokenService _tokens) : Endpoint<LoginRequest, AuthResponse>
{
	public override void Configure()
	{
		Post("/auth/login");
		AllowAnonymous();
	}

	public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
	{
		var result = await _users.LoginAsync(req.Username, req.PreHashedPassword, req.DeviceName, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
		if (result.RefreshToken is null)
		{
			AddError("RefreshToken null");
			await Send.ErrorsAsync(cancellation: ct);
			return;
		}		

		if (!result.Success)
		{
			foreach (var e in result.Errors)
				AddError(e);
			await Send.ErrorsAsync(cancellation: ct);
			return;
		}

		var user = await _users.FindUserByUsernameAsync(req.Username);
		if (!user.EmailVerified && !user.IsDeletionRequested)
		{
			await _users.FlagUserForDeletionAsync(user.UserId);
			AddError("Email address never verified.");
			await Send.ErrorsAsync(cancellation: ct);
			return;
		}

		if (user.IsDeletionRequested)
		{
			AddError("Account is pending deletion, it cannot be logged into at this time.");
			await Send.ErrorsAsync(cancellation: ct);
			return;
		}

		if (user.TwoFactorEnabled)
		{
			await Send.OkAsync(new AuthResponse()
			{
				TwoFactorRequired = true,
				TwoFactorPendingToken = _tokens.GenerateTwoFactorPendingToken(user)
			});
			return;
		}

		await Send.OkAsync(new AuthResponse()
		{
			AccessToken = result.AccessToken,
			RefreshToken = result.RefreshToken!.Token,
			RefreshTokenId = result.RefreshToken.Id
		});
	}
}

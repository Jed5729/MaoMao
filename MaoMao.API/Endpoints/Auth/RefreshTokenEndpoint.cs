using MaoMao.API.DTO.Auth;
using MaoMao.API.Services.Contract;
using MaoMao.Shared.DTO.Auth;

namespace MaoMao.API.Endpoints.Auth;

public class RefreshTokenEndpoint(IUserService users, ITokenService tokens) : Endpoint<RefreshTokenRequest, AuthResponse>
{
	public override void Configure()
	{
		Post("/auth/refresh");
		AllowAnonymous();
	}

	public override async Task HandleAsync(RefreshTokenRequest req, CancellationToken ct)
	{
		var storedSession = await tokens.GetByTokenAsync(req.RefreshToken);

		if (storedSession is null)
		{
			await Send.UnauthorizedAsync();
			return;
		}

		var user = await users.FindUserByIdAsync(storedSession.UserId);
		if (user is null)
		{
			await Send.UnauthorizedAsync();
			return;
		}
		else if (!user.EmailVerified && !user.IsDeletionRequested)
		{
			await users.FlagUserForDeletionAsync(user.UserId);
			AddError("Email address never verified.");
			await Send.ErrorsAsync(cancellation: ct);
			return;
		}

		var refreshToken = await tokens.GenerateRefreshTokenAsync(user, req.DeviceName, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
		var accessToken = tokens.GenerateAccessToken(user, refreshToken.Id);
		await tokens.RevokeTokenAsync(storedSession.Id);

		await Send.OkAsync(new AuthResponse()
		{
			AccessToken = accessToken,
			RefreshToken = refreshToken.Token,
			RefreshTokenId = refreshToken.Id
		});
	}
}

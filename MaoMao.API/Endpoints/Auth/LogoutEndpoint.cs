using MaoMao.API.DTO.Auth;
using MaoMao.API.Services.Contract;
using MaoMao.Shared.DTO.Auth;

namespace MaoMao.API.Endpoints.Auth;

public class LogoutEndpoint(ITokenService tokens) : Endpoint<LogoutRequest>
{
	public override void Configure()
	{
		Post("/auth/logout");
		AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
	}

	public override async Task HandleAsync(LogoutRequest req, CancellationToken ct)
	{
		if (req.RefreshTokenId == string.Empty) req.RefreshTokenId = User.FindFirstValue(JwtRegisteredClaimNames.Sid) ?? "";
		var storedSession = await tokens.GetByIdAsync(req.RefreshTokenId);
		if (storedSession is null)
		{
			await Send.UnauthorizedAsync();
			return;
		}
		else if (storedSession.UserId != User.FindFirstValue(JwtRegisteredClaimNames.Sub))
		{
			await Send.UnauthorizedAsync();
			return;
		}

		await tokens.RevokeTokenAsync(req.RefreshTokenId);
		await Send.OkAsync();
	}
}

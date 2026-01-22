using MaoMao.API.DTO.User;
using MaoMao.API.Services.Contract;

namespace MaoMao.API.Endpoints.Auth;

public class LogoutAllEndpoint(ITokenService tokens) : EndpointWithoutRequest
{
	public override void Configure()
	{
		Get("/auth/logoutall");
		AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var sessions = await tokens.GetActiveSessionsAsync(User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? "", User.FindFirstValue(JwtRegisteredClaimNames.Sid) ?? "");
		var sessionsList = sessions.ToList();
		if (sessionsList.Count > 1)
		{
			var loggedOutCount = 0;
			foreach(Session s in sessions)
			{
				if (s.IsCurrentSession) continue;
				await tokens.RevokeTokenAsync(s.Id);
				loggedOutCount++;
			}
			await Send.OkAsync($"Logged out {loggedOutCount} sessions");
			return;
		}
		else
		{
			AddError("No other sessions found to log out from");
			await Send.ErrorsAsync();
			return;
		}
	}
}

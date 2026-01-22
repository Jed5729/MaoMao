using MaoMao.API.DTO.User;
using MaoMao.API.Services.Contract;

namespace MaoMao.API.Endpoints.User;

public class GetSessionsEndpoint(ITokenService tokenService) : EndpointWithoutRequest<GetSessionsResponse>
{
	public override void Configure()
	{
		Get("/user/sessions");
		AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
		if (string.IsNullOrWhiteSpace(userId))
		{
			AddError("Null user ID in claims");
			await Send.ErrorsAsync(cancellation: ct);
			return;
		}

		var sessions = await tokenService.GetActiveSessionsAsync(userId, User.FindFirstValue(JwtRegisteredClaimNames.Sid) ?? "");
		await Send.OkAsync(new GetSessionsResponse()
		{
			Sessions = sessions.ToList()
		});
	}
}

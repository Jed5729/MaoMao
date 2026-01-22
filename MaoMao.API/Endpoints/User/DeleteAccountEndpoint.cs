using MaoMao.API.Services.Contract;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MaoMao.API.Endpoints.User;

public class DeleteAccountEndpoint(IUserService _users) : EndpointWithoutRequest
{
	public override void Configure()
	{
		Get("/user/delete-account");
		AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
		if(userId is null)
		{
			AddError("Null user ID in claims");
			await Send.ErrorsAsync(cancellation: ct);
			return;
		}

		var result = await _users.FlagUserForDeletionAsync(userId);

		if (!result.Success)
		{
			foreach (var e in result.Errors)
				AddError(e);
			await Send.ErrorsAsync(cancellation: ct);
			return;
		}

		await Send.OkAsync(ct);
	}
}
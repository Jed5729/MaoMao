namespace MaoMao.API.Endpoints.Status;

[Throttle(hitLimit: 10, durationSeconds: 60)] // 10 requests per minute
public class HelloEndpoint : EndpointWithoutRequest
{
	public override void Configure()
	{
		Get("/status/hello");
		AllowAnonymous();
	}

	public override async Task HandleAsync(CancellationToken ct)
	{
		await Send.OkAsync(ct);
	}
}

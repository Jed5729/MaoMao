using MaoMao.Shared.DTO.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace MaoMao.Services;

public class AuthManager
{
	//private const string AccessTokenKey = "accessToken";
	private const string RefreshTokenKey = "refreshToken";
	private const string RefreshTokenIdKey = "refreshTokenId";
	private const string HttpClientName = "ApiClient";
	public static string AccessToken { get; private set; } = string.Empty;

	private readonly HttpClient _http;

	public AuthManager(IHttpClientFactory httpFactory)
	{
		_http = httpFactory.CreateClient(HttpClientName);
	}

	private string PreHashPassword(string password) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

	private string GetDeviceName() => $"{DeviceInfo.Current.Platform}: {DeviceInfo.Current.Name}";

	public async Task<AuthResponse?> LoginUserAsync(string username, string password)
	{
		var request = new LoginRequest
		{
			Username = username,
			PreHashedPassword = PreHashPassword(password),
			DeviceName = GetDeviceName()
		};

		var response = await _http.PostAsJsonAsync("auth/login", request);

		response.EnsureSuccessStatusCode();

		var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

		if (result is null) return result;
		
		AccessToken = result.AccessToken;
		await SecureStorage.SetAsync(RefreshTokenKey, result.RefreshToken);
		await SecureStorage.SetAsync(RefreshTokenIdKey, result.RefreshTokenId);

		return result;
	}

	public async Task<bool> LogoutUserAsync()
	{
		var refreshTokenId = await SecureStorage.GetAsync(RefreshTokenIdKey);

		if (string.IsNullOrWhiteSpace(refreshTokenId)) return false;

		var request = new LogoutRequest() { RefreshTokenId = refreshTokenId };

		var response = await _http.PostAsJsonAsync("auth/logout", request);

		response.EnsureSuccessStatusCode();

		return true;
	}

	public async Task<AuthResponse?> RegisterUserAsync(string username, string password, string email)
	{
		var request = new RegisterUserRequest()
		{
			Username = username,
			PreHashedPassword = PreHashPassword(password),
			Email = email,
			DeviceName = GetDeviceName()
		};

		var response = await _http.PostAsJsonAsync("auth/register", request);

		response.EnsureSuccessStatusCode();

		var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

		if (result is null) return result;

		AccessToken = result.AccessToken;
		await SecureStorage.SetAsync(RefreshTokenKey, result.RefreshToken);
		await SecureStorage.SetAsync(RefreshTokenIdKey, result.RefreshTokenId);

		return result;
	}

	public async Task<AuthResponse?> RefreshTokenAsync()
	{
		var refreshToken = await SecureStorage.GetAsync(RefreshTokenKey);
		if (refreshToken is null) return null;

		var request = new RefreshTokenRequest()
		{
			DeviceName = GetDeviceName(),
			RefreshToken = refreshToken
		};

		var response = await _http.PostAsJsonAsync("auth/refresh", request);

		response.EnsureSuccessStatusCode();

		var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
		if (result is null) return result;

		AccessToken = result.AccessToken;
		await SecureStorage.SetAsync(RefreshTokenKey, result.RefreshToken);
		await SecureStorage.SetAsync(RefreshTokenIdKey, result.RefreshTokenId);

		return result;
	}

	public async Task SendVerificationAsync()
	{
		var response = await _http.GetAsync("user/sendverification");

		response.EnsureSuccessStatusCode();
	}

	public class AuthHandler() : DelegatingHandler
	{
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			//if (string.IsNullOrWhiteSpace(AccessToken))
			//{
			//	var result = await services.GetService<AuthManager>()!.RefreshTokenAsync();

			//	if (result is not null)
			//	{
			//		AccessToken = result.AccessToken;
			//	}
			//}

			if (!string.IsNullOrWhiteSpace(AccessToken))
			{
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
			}

			return await base.SendAsync(request, cancellationToken);
		}
	}
}

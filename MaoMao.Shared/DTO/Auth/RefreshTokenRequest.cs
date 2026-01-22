namespace MaoMao.API.DTO.Auth;

public class RefreshTokenRequest
{
	public string RefreshToken { get; set; } = string.Empty;
	public string DeviceName { get; set; } = "Unknown";
}

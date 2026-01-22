namespace MaoMao.API.DTO.Auth;

public class LoginRequest
{
	public string Username { get; set; } = "";
	public string PreHashedPassword { get; set; } = "";
	public string DeviceName { get; set; } = "Unknown";
}

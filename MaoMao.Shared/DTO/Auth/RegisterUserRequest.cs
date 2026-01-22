namespace MaoMao.API.DTO.Auth;

public class RegisterUserRequest
{
	public string Username { get; set; } = null!;
	public string PreHashedPassword { get; set; } = null!;
	public string Email { get; set; } = null!;
	public string DeviceName { get; set; } = "Unknown";
}

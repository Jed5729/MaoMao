namespace MaoMao.Shared.DTO.Auth;

public class VerifyTwoFactorRequest
{
	public string Code { get; set; } = string.Empty;
	public string Device { get; set; } = "Unknown";
}

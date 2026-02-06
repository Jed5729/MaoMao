namespace MaoMao.Shared.DTO.Auth;

public class AuthResponse
{
	public string AccessToken { get; set; } = string.Empty;
	public string RefreshToken { get; set; } = string.Empty;
	public string RefreshTokenId { get; set; } = string.Empty;
	public bool TwoFactorRequired { get; set; } = false;
	public string TwoFactorPendingToken { get; set; } = string.Empty;
}

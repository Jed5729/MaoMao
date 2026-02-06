namespace MaoMao.Shared.DTO.User;

public class EnableTwoFactorResponse
{
	public string Secret { get; set; } = string.Empty;
	public string QRUri { get; set; } = string.Empty;
}

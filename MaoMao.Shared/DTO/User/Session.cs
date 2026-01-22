namespace MaoMao.API.DTO.User;

public class Session
{
	public string Id { get; set; } = string.Empty;
	public string DeviceName { get; set; } = string.Empty;
	public string IpAddress { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime ExpiresAt { get; set; } = DateTime.UtcNow;
	public bool IsCurrentSession { get; set; } = false;
}

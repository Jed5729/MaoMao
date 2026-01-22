using MongoDB.Bson.Serialization.Attributes;

namespace MaoMao.API.Models;

public class RefreshToken
{
	[BsonId]
	public string Id { get; set; } = Guid.NewGuid().ToString();

	[BsonElement("token")]
	public string Token { get; set; } = string.Empty;

	[BsonElement("userId")]
	public string UserId { get; set; } = string.Empty;

	[BsonElement("deviceName")]
	public string DeviceName { get; set; } = string.Empty;

	[BsonElement("ipAddress")]
	public string IpAddress { get; set; } = string.Empty;

	[BsonElement("createdAt")]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	[BsonElement("expiresAt")]
	public DateTime ExpiresAt { get; set; } = DateTime.UtcNow;
}

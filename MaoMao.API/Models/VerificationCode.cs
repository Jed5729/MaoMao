using MongoDB.Bson.Serialization.Attributes;

namespace MaoMao.API.Models;

public class VerificationCode
{
	[BsonId]
	public string Id { get; set; } = Guid.NewGuid().ToString();

	[BsonElement("userId")]
	public string UserId { get; set; } = string.Empty;

	[BsonElement("code")]
	public int Code { get; set; }

	[BsonElement("timeIssued")]
	public DateTime TimeIssued { get; set; } = DateTime.UtcNow;
}

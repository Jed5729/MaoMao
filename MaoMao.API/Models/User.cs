using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MaoMao.API.Models;

public class User
{
	[BsonId]
	[BsonRepresentation(BsonType.ObjectId)]
	public string UserId { get; set; } = ObjectId.GenerateNewId().ToString();

	[BsonElement("username")]
	public string Username { get; set; } = null!;

	[BsonElement("passwordHash")]
	public string PasswordHash { get; set; } = null!;

	[BsonElement("email")]
	public string Email { get; set; } = null!;

	[BsonElement("emailVerified")]
	public bool EmailVerified { get; set; } = false;

	[BsonElement("createdDate")]
	public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

	[BsonElement("lastUpdatedDate")]
	public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;

	[BsonElement("isDeletionRequested")]
	public bool IsDeletionRequested { get; set; } = false;

	[BsonElement("deletionRequestedTime")]
	public DateTime? DeletionRequestedTime { get; set; } = null;

	[BsonElement("twoFactorEnabled")]
	public bool TwoFactorEnabled { get; set; } = false;

	[BsonElement("twoFactorSecret")]
	public string TwoFactorSecret { get; set; } = null!;

	[BsonElement("knownIPs")]
	public List<string> KnownIPs { get; set; } = new();

	[BsonElement("isAdmin")]
	public bool IsAdmin { get; set; } = false;
}

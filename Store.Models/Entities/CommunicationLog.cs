using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class CommunicationLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public Guid? UserId { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public CommunicationChannel Channel { get; set; }
    public CommunicationStatus Status { get; set; }
    public string Payload { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime? DateUpdated { get; set; }
}

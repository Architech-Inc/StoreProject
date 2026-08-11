using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class AuditLog : BaseEntity
{
    public long AuditLogId { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public User User { get; set; } = null!;
}

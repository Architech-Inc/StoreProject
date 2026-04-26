using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class ChangeLog : BaseEntity
{
    public long ChangeLogId { get; set; }
    public Guid UserId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public ChangeLogAction Action { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }

    public User User { get; set; } = null!;
}

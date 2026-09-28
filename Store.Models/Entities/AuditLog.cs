using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class AuditLog : BaseEntity
{
    public long AuditLogId { get; set; }
    public Guid UserId { get; set; }

    /// <summary>
    /// MT-05 — Tenant isolation. NULL for system-level / pre-provisioning audits.
    /// When set, every audit read MUST filter by this column so cross-tenant
    /// audit leakage is impossible at the data layer (not just in the UI).
    /// </summary>
    public Guid? TenantId { get; set; }

    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    public User User { get; set; } = null!;
}

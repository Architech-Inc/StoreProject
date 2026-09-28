using System.ComponentModel.DataAnnotations;

namespace Store.Models.DTOs.Audit;

public class AuditLogDto
{
    public long Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Category { get; set; } = "System";
    public string Severity { get; set; } = "Info"; // Info, Warning, Critical, Security
    public Guid ActorUserId { get; set; }
    public string ActorUsername { get; set; } = string.Empty;
    public string? ActorRole { get; set; }
    public string? ActorFullName { get; set; }

    /// <summary>
    /// MT-05 — tenant scope of the audit entry. NULL for system-level /
    /// pre-provisioning audits (signup, login before tenant resolution).
    /// </summary>
    public Guid? TenantId { get; set; }

    public string Summary { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceType { get; set; }
    public string? TargetEntity { get; set; }
    public string? TargetId { get; set; }
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public string? MetadataJson { get; set; }
    public string? RawDetails { get; set; }
    public DateTime DateCreated { get; set; }
}

public class AuditLogMetricsDto
{
    public int TotalEvents { get; set; }
    public int TodayEvents { get; set; }
    public int SecurityIncidentsCount { get; set; }
    public int PrivilegeChangesCount { get; set; }
    public int CriticalRiskCount { get; set; }
}

public class AuditLogFilterRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 30;
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public string? Severity { get; set; }
    public Guid? UserId { get; set; }
    public string? TargetEntity { get; set; }

    /// <summary>
    /// MT-05 — restrict reads to a single tenant. NULL means "system +
    /// all tenants" (caller must have cross-tenant authority to use this).
    /// When set, the service must filter on TenantId == filter value.
    /// </summary>
    public Guid? TenantId { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class CreateAuditLogEntryRequest
{
    [Required]
    public Guid UserId { get; set; }

    /// <summary>
    /// MT-05 — tenant scope of this audit entry. NULL for system-level /
    /// pre-provisioning audits. The service writes this verbatim into the
    /// <c>tenant_id</c> column so the composite index can serve tenant
    /// reads.
    /// </summary>
    public Guid? TenantId { get; set; }

    [Required, StringLength(100)]
    public string Action { get; set; } = string.Empty;

    public string Category { get; set; } = "System";
    public string Severity { get; set; } = "Info";
    public string Summary { get; set; } = string.Empty;
    public string? TargetEntity { get; set; }
    public string? TargetId { get; set; }
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public string? MetadataJson { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class StructuredAuditPayload
{
    public string Category { get; set; } = "System";
    public string Severity { get; set; } = "Info";
    public string Summary { get; set; } = string.Empty;
    public string? TargetEntity { get; set; }
    public string? TargetId { get; set; }
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public string? MetadataJson { get; set; }
}

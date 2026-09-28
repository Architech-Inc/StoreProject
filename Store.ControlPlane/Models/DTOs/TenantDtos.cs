using System.ComponentModel.DataAnnotations;
using TenantTier = Store.Models.Billing.TenantTier;

namespace Store.ControlPlane.Models.DTOs;

public class ProvisionTenantRequest
{
    [Required, StringLength(100, MinimumLength = 3)]
    public string StoreName { get; set; } = string.Empty;

    [Required, RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase alphanumeric characters and hyphens.")]
    [StringLength(50, MinimumLength = 3)]
    public string Slug { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string AdminEmail { get; set; } = string.Empty;

    [Required, StringLength(50, MinimumLength = 3)]
    public string AdminUsername { get; set; } = "admin";

    [Required, StringLength(100, MinimumLength = 8)]
    public string AdminPassword { get; set; } = string.Empty;

    [StringLength(10)]
    public string Currency { get; set; } = "XAF";

    [Required]
    public TenantTier PlanTier { get; set; } = TenantTier.Professional;

    public Guid? ReleaseId { get; set; }

    public string? CustomDomain { get; set; }
}

/// <summary>MT-01 — async-provisioning wrapper. The portal submits a job and polls its status.</summary>
public class ProvisionTenantAsyncRequest : ProvisionTenantRequest
{
    /// <summary>The portal account that will own the tenant. Required for the Account → Tenant link.</summary>
    [Required]
    public Guid AccountId { get; set; }
}

/// <summary>MT-01 — what the portal polls for. JobId round-trip + terminal status.</summary>
public class ProvisioningJobDto
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? StatusDetail { get; set; }
    public string? FailureReason { get; set; }
    public Guid? TenantId { get; set; }
    public string? TenantSlug { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class TenantDto
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public TenantStatus Status { get; set; }
    public TenantTier PlanTier { get; set; }
    public string? CustomDomain { get; set; }
    public string UiUrl { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
    public DateTime? LastHealthCheck { get; set; }
    public bool IsHealthy { get; set; }
    public string? LastHealthMessage { get; set; }
}

public class TenantDetailDto : TenantDto
{
    public List<TenantProvisioningLog> ProvisioningLogs { get; set; } = new();
}

public class TenantHealthSummaryDto
{
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int ProvisioningTenants { get; set; }
    public int SuspendedTenants { get; set; }
    public int FailedTenants { get; set; }
    public int HealthyCount { get; set; }
    public int UnhealthyCount { get; set; }
}

public record SlugCheckDto(
    string Slug,
    bool IsAvailable,
    string? Reason = null
);

public class SystemReleaseDto
{
    public Guid ReleaseId { get; set; }
    public string VersionName { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public bool IsPublic { get; set; }
    public string ReleaseNotes { get; set; } = string.Empty;
}

public class TenantSnapshotDto
{
    public Guid SnapshotId { get; set; }
    public Guid TenantId { get; set; }
    public Guid? ReleaseId { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long SizeBytes { get; set; }
}

public class TenantSdlcStatusDto
{
    public Guid TenantId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public Guid? CurrentReleaseId { get; set; }
    public SystemReleaseDto? CurrentRelease { get; set; }
    public string EnvironmentType { get; set; } = "Production";
    public Guid? ParentTenantId { get; set; }
    public string? ParentSlug { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public List<SystemReleaseDto> AvailableReleases { get; set; } = new();
    public List<TenantSnapshotDto> Snapshots { get; set; } = new();
    public List<SandboxSummaryDto> Sandboxes { get; set; } = new();
}

public class SandboxSummaryDto
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string UiUrl { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public Guid? ReleaseId { get; set; }
    public string? ReleaseVersion { get; set; }
    public DateTime DateCreated { get; set; }
    public bool IsHealthy { get; set; }
}

// ─── MT-07 — public tenant status / maintenance schedule ───────────────────

public class MaintenanceWindowDto
{
    public Guid MaintenanceWindowId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public string Severity { get; set; } = "Info"; // Info | Warning | Critical
    public bool IsResolved { get; set; }
    public DateTime DateCreated { get; set; }
}

/// <summary>
/// MT-07 — anonymous-facing tenant status payload. Returned by the public
/// /api/public/tenants/{slug}/status endpoint. NEVER includes secrets,
/// connection strings, internal ids of other tenants, or anything else that
/// should not leave the platform.
/// </summary>
public class TenantStatusDto
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Active | Provisioning | Suspended | ...
    public bool IsHealthy { get; set; }
    public DateTime? LastHealthCheckUtc { get; set; }
    public string? LastHealthMessage { get; set; }

    /// <summary>Active windows — start ≤ now ≤ end, not resolved. Empty if none.</summary>
    public List<MaintenanceWindowDto> ActiveMaintenance { get; set; } = new();

    /// <summary>Upcoming windows — start > now, not resolved. Empty if none.</summary>
    public List<MaintenanceWindowDto> UpcomingMaintenance { get; set; } = new();

    /// <summary>Recently completed windows (last 30 days, for context).</summary>
    public List<MaintenanceWindowDto> RecentMaintenance { get; set; } = new();

    /// <summary>True unless an active Critical window exists.</summary>
    public bool IsAcceptingTraffic { get; set; } = true;

    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}

public class CreateMaintenanceWindowRequest
{
    [Required, StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DateTime StartUtc { get; set; }

    [Required]
    public DateTime EndUtc { get; set; }

    public MaintenanceSeverity Severity { get; set; } = MaintenanceSeverity.Info;
}


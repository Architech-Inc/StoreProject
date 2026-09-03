using System.ComponentModel.DataAnnotations;

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


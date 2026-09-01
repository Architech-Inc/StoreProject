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

    public TenantTier PlanTier { get; set; } = TenantTier.Professional;

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

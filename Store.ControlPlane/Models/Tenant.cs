namespace Store.ControlPlane.Models;

public class Tenant
{
    public Guid TenantId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
    public string Currency { get; set; } = "XAF";
    public TenantStatus Status { get; set; } = TenantStatus.Pending;
    public TenantTier PlanTier { get; set; } = TenantTier.Professional;
    public string CustomDomain { get; set; } = string.Empty;
    public string UiUrl { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public TenantSecrets Secrets { get; set; } = new();
    public List<TenantProvisioningLog> ProvisioningLogs { get; set; } = new();
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime? LastHealthCheck { get; set; }
    public bool IsHealthy { get; set; }
    public string? LastHealthMessage { get; set; }
}

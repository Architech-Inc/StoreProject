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
    public TenantDomainConfig DomainConfig { get; set; } = new();
    public List<TenantBranchMapping> Branches { get; set; } = new();
    public List<TenantProvisioningLog> ProvisioningLogs { get; set; } = new();
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime? LastHealthCheck { get; set; }
    public bool IsHealthy { get; set; }
    public string? LastHealthMessage { get; set; }
}

public class TenantDomainConfig
{
    public string CustomDomain { get; set; } = string.Empty;
    public DomainStatus Status { get; set; } = DomainStatus.NotConfigured;
    public string VerificationToken { get; set; } = string.Empty;
    public string VerificationRecordName { get; set; } = string.Empty;
    public DateTime? VerifiedAt { get; set; }
    public DateTime? LastCheckedAt { get; set; }
    public string? LastErrorMessage { get; set; }
}

public enum DomainStatus
{
    NotConfigured = 0,
    Pending = 1,
    Verified = 2,
    Failed = 3
}

public class TenantBranchMapping
{
    public Guid BranchId { get; set; } = Guid.NewGuid();
    public string BranchName { get; set; } = string.Empty;
    public string BranchSlug { get; set; } = string.Empty;
    public BranchDomainType DomainType { get; set; } = BranchDomainType.Platform;
    public string? CustomSubdomain { get; set; }
    public string ResolvedUrl { get; set; } = string.Empty;
    public DomainStatus VerificationStatus { get; set; } = DomainStatus.Verified;
    public string VerificationRecordName { get; set; } = string.Empty;
    public string VerificationRecordValue { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}

public enum BranchDomainType
{
    Platform = 0, // [branch].[slug].store.domain
    Custom = 1    // [branch].[customdomain]
}

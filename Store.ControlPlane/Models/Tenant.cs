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
    public List<BackupProviderConfig> BackupProviders { get; set; } = new();
    public BackupScheduleConfig BackupSchedule { get; set; } = new();
    public List<TenantBackupJobRecord> BackupHistory { get; set; } = new();
    public List<TenantAuditRecord> AuditTrail { get; set; } = new();
    public List<TenantProvisioningLog> ProvisioningLogs { get; set; } = new();
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime? LastHealthCheck { get; set; }
    public bool IsHealthy { get; set; }
    public string? LastHealthMessage { get; set; }

    // SDLC & Sandbox Fields
    public Guid? CurrentReleaseId { get; set; }
    public TenantEnvironmentType EnvironmentType { get; set; } = TenantEnvironmentType.Production;
    public Guid? ParentTenantId { get; set; }
    public DateTime? LastAccessedAt { get; set; }
}

public enum TenantEnvironmentType
{
    Production = 0,
    Sandbox = 1
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
    Platform = 0,
    Custom = 1
}

// Backup Models

public class BackupProviderConfig
{
    public BackupProviderType ProviderType { get; set; }
    public string AccountEmail { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public bool IsConnected { get; set; }
    public string EncryptedAccessToken { get; set; } = string.Empty;
    public string EncryptedRefreshToken { get; set; } = string.Empty;
    public DateTime? TokenExpiresAt { get; set; }
    public S3StorageConfig? S3Config { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public DateTime? LastBackupAt { get; set; }
    public string? LastBackupStatus { get; set; }
}

public enum BackupProviderType
{
    OneDrive = 0,
    GoogleDrive = 1,
    S3 = 2,
    Local = 3
}

public class S3StorageConfig
{
    public string EndpointUrl { get; set; } = "https://s3.amazonaws.com";
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public string AccessKeyId { get; set; } = string.Empty;
    public string EncryptedSecretKey { get; set; } = string.Empty;
}

public class BackupScheduleConfig
{
    public BackupFrequency Frequency { get; set; } = BackupFrequency.Daily;
    public int RetentionCount { get; set; } = 14;
    public bool IsEnabled { get; set; } = true;
    public DateTime? NextRunAt { get; set; }
}

public enum BackupFrequency
{
    Manual = 0,
    Hourly = 1,
    Daily = 2,
    Weekly = 3
}

public class TenantBackupJobRecord
{
    public Guid BackupId { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public long TotalSizeBytes { get; set; }
    public List<string> Files { get; set; } = new();
    public string DestinationProviders { get; set; } = string.Empty;
    public string Status { get; set; } = "Completed";
    public string? ErrorMessage { get; set; }
    public int RetentionDays { get; set; } = 30;
}

// Audit Models

public class TenantAuditRecord
{
    public Guid AuditId { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string ActorEmail { get; set; } = "system";
    public string ActionType { get; set; } = string.Empty; // ContainerRestart, DomainRegistered, DomainVerified, BranchAdded, BranchRemoved, BackupTriggered, ProviderConnected, SiloSuspended, SiloResumed
    public string Details { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
}

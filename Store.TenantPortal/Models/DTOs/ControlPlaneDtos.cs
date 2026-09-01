namespace Store.TenantPortal.Models.DTOs;

public record ApiResponse<T>(
    bool Success,
    string Message,
    T Data,
    Dictionary<string, string[]>? Errors = null
);

public record PortalAuthDto(
    Guid AccountId,
    string Email,
    string FullName,
    Guid? TenantId,
    string? TenantSlug,
    string? TenantName,
    string SessionToken,
    DateTime ExpiresAt
);

public record SlugCheckDto(
    string Slug,
    bool IsAvailable,
    string? Reason = null
);

public record ProvisionTenantDto(
    string Name,
    string Slug,
    string AdminEmail,
    string AdminUsername,
    string AdminPassword,
    string Currency,
    int PlanTier,
    string? CustomDomain = null
);

public record TenantSummaryDto(
    Guid TenantId,
    string Name,
    string Slug,
    string AdminEmail,
    string AdminUsername,
    string Currency,
    string Status,
    string PlanTier,
    string? CustomDomain,
    string UiUrl,
    string ApiUrl,
    bool IsHealthy,
    DateTime? LastHealthCheck,
    string? LastHealthMessage,
    DateTime DateCreated
);

public record TenantDetailDto(
    Guid TenantId,
    string Name,
    string Slug,
    string AdminEmail,
    string AdminUsername,
    string Currency,
    string Status,
    string PlanTier,
    string? CustomDomain,
    string UiUrl,
    string ApiUrl,
    bool IsHealthy,
    DateTime? LastHealthCheck,
    string? LastHealthMessage,
    DateTime DateCreated,
    List<TenantProvisioningLogDto> ProvisioningLogs
);

public record TenantProvisioningLogDto(
    DateTime Timestamp,
    string Phase,
    string Message,
    bool IsError = false
);

// Phase 2 DTOs: Environment, Domains & Branches

public record EnvironmentStatusDto(
    Guid TenantId,
    string TenantName,
    string Slug,
    string Status,
    bool IsHealthy,
    DateTime? LastHealthCheck,
    string? LastHealthMessage,
    List<ContainerStatusDto> Containers
);

public record ContainerStatusDto(
    string Name,
    string ContainerName,
    string ServiceType,
    string Image,
    string Status,
    bool IsHealthy,
    DateTime? LastChecked
);

public record TenantDomainDto(
    Guid TenantId,
    string Slug,
    string PlatformUiUrl,
    string PlatformApiUrl,
    string CustomDomain,
    string CustomDomainStatus,
    string VerificationRecordName,
    string VerificationRecordValue,
    DateTime? CustomDomainVerifiedAt,
    string? LastErrorMessage
);

public record SetCustomDomainRequest(
    string Domain
);

public record VerifyDomainResponse(
    string Domain,
    bool IsVerified,
    string Status,
    string? CheckedHost,
    string? ExpectedValue,
    List<string>? FoundValues,
    string? Message
);

public record BranchDto(
    Guid BranchId,
    string BranchName,
    string BranchSlug,
    string DomainType,
    string? CustomSubdomain,
    string ResolvedUrl,
    string VerificationStatus,
    string VerificationRecordName,
    string VerificationRecordValue,
    DateTime DateCreated
);

public record CreateBranchRequest(
    string BranchName,
    string BranchSlug,
    string DomainType = "Platform",
    string? CustomSubdomain = null
);

// Phase 3 DTOs: Backups & Cloud Storage

public record BackupSummaryDto(
    Guid TenantId,
    string Slug,
    BackupScheduleDto Schedule,
    List<BackupProviderDto> Providers,
    List<BackupJobDto> RecentBackups
);

public record BackupProviderDto(
    string ProviderType,
    string DisplayName,
    bool IsConnected,
    string? AccountEmail,
    string? AccountName,
    DateTime? ConnectedAt,
    DateTime? LastBackupAt,
    string? LastBackupStatus
);

public record BackupScheduleDto(
    string Frequency,
    int RetentionCount,
    bool IsEnabled,
    DateTime? NextRunAt
);

public record BackupJobDto(
    Guid BackupId,
    DateTime Timestamp,
    long TotalSizeBytes,
    string FormattedSize,
    List<string> Files,
    string DestinationProviders,
    string Status,
    string? ErrorMessage
);

public record ConfigureS3Request(
    string EndpointUrl,
    string BucketName,
    string Region,
    string AccessKeyId,
    string SecretAccessKey
);

public record SaveOAuthTokensRequest(
    string ProviderType,
    string AccessToken,
    string RefreshToken,
    string AccountEmail,
    string? AccountName,
    int ExpiresInSeconds = 3600
);

public record UpdateScheduleRequest(
    string Frequency,
    int RetentionCount,
    bool IsEnabled
);

public record TriggerBackupResponse(
    Guid BackupId,
    string Status,
    string Message,
    long TotalSizeBytes,
    List<string> Files,
    DateTime Timestamp
);

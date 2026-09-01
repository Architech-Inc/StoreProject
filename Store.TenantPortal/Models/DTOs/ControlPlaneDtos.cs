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

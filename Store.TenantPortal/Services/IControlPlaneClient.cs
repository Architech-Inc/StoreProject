using Store.TenantPortal.Models.DTOs;

namespace Store.TenantPortal.Services;

public interface IControlPlaneClient
{
    // Auth & Slugs
    Task<SlugCheckDto> CheckSlugAvailabilityAsync(string slug, CancellationToken ct = default);
    Task<PortalAuthDto> RegisterAccountAsync(string email, string fullName, string password, CancellationToken ct = default);
    Task<PortalAuthDto?> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<PortalAuthDto?> GetAccountAsync(Guid accountId, CancellationToken ct = default);
    Task<TenantSummaryDto> ProvisionTenantAsync(ProvisionTenantDto request, CancellationToken ct = default);
    Task<TenantDetailDto?> GetTenantDetailsAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> CheckTenantHealthAsync(Guid tenantId, CancellationToken ct = default);
    Task LinkAccountToTenantAsync(Guid accountId, Guid tenantId, CancellationToken ct = default);

    // Environment Control
    Task<EnvironmentStatusDto?> GetEnvironmentStatusAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> RestartServiceAsync(Guid tenantId, string serviceName, CancellationToken ct = default);
    Task<bool> SuspendTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> ResumeTenantAsync(Guid tenantId, CancellationToken ct = default);

    // Domains
    Task<TenantDomainDto?> GetDomainConfigAsync(Guid tenantId, CancellationToken ct = default);
    Task<TenantDomainDto> SetCustomDomainAsync(Guid tenantId, string domain, CancellationToken ct = default);
    Task<VerifyDomainResponse> VerifyCustomDomainAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> RemoveCustomDomainAsync(Guid tenantId, CancellationToken ct = default);

    // Branches
    Task<IReadOnlyList<BranchDto>> GetBranchesAsync(Guid tenantId, CancellationToken ct = default);
    Task<BranchDto> AddBranchAsync(Guid tenantId, CreateBranchRequest request, CancellationToken ct = default);
    Task<VerifyDomainResponse> VerifyBranchAsync(Guid tenantId, Guid branchId, CancellationToken ct = default);
    Task<bool> RemoveBranchAsync(Guid tenantId, Guid branchId, CancellationToken ct = default);

    // Backups
    Task<BackupSummaryDto?> GetBackupSummaryAsync(Guid tenantId, CancellationToken ct = default);
    Task<TriggerBackupResponse> TriggerBackupAsync(Guid tenantId, CancellationToken ct = default);
    Task<BackupProviderDto> ConfigureS3ProviderAsync(Guid tenantId, ConfigureS3Request request, CancellationToken ct = default);
    Task<BackupProviderDto> SaveOAuthTokensAsync(Guid tenantId, SaveOAuthTokensRequest request, CancellationToken ct = default);
    Task<bool> DisconnectBackupProviderAsync(Guid tenantId, string providerType, CancellationToken ct = default);
    Task<BackupScheduleDto> UpdateBackupScheduleAsync(Guid tenantId, UpdateScheduleRequest request, CancellationToken ct = default);

    // Audit Trail
    Task<IReadOnlyList<TenantAuditDto>> GetAuditTrailAsync(Guid tenantId, int limit = 50, CancellationToken ct = default);
}

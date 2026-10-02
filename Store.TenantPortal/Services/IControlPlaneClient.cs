using Store.TenantPortal.Models.DTOs;
// MT-02 — re-export the PayDunya response DTO so callers don't need
// a separate using for Store.Models.DTOs.Payments.
using CreateInvoiceResponse = Store.Models.DTOs.Payments.CreateInvoiceResponse;

namespace Store.TenantPortal.Services;

public interface IControlPlaneClient
{
    // Auth & Slugs
    Task<SlugCheckDto> CheckSlugAvailabilityAsync(string slug, CancellationToken ct = default);
    Task<PortalAuthDto> RegisterAccountAsync(string email, string fullName, string password, CancellationToken ct = default);
    Task<PortalAuthDto?> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<PortalAuthDto?> GetAccountAsync(Guid accountId, CancellationToken ct = default);
    Task<TenantSummaryDto> ProvisionTenantAsync(ProvisionTenantDto request, CancellationToken ct = default);

    // MT-01 — async provisioning (preferred): submit returns a JobId; portal polls.
    Task<ProvisioningJobResponse> ProvisionTenantAsyncJobAsync(ProvisionTenantDto request, Guid accountId, CancellationToken ct = default);
    Task<ProvisioningJobResponse?> GetProvisioningJobAsync(Guid jobId, CancellationToken ct = default);
    Task<bool> RetryProvisioningJobAsync(Guid jobId, CancellationToken ct = default);
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

    // SDLC & Sandboxing
    Task<TenantSdlcStatusDto?> GetSdlcStatusAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<SystemReleaseDto>> GetReleasesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TenantSnapshotDto>> GetSnapshotsAsync(string slug, CancellationToken ct = default);
    Task<bool> UpgradeTenantAsync(string slug, Guid releaseId, CancellationToken ct = default);
    Task<bool> RollbackTenantAsync(string slug, Guid snapshotId, CancellationToken ct = default);
    Task<TenantSummaryDto?> CreateSandboxAsync(string slug, Guid releaseId, bool maskData = true, CancellationToken ct = default);
    Task<bool> DeleteSandboxAsync(string slug, string sandboxSlug, CancellationToken ct = default);

    // MT-07 — public tenant status + maintenance windows.
    // GetTenantPublicStatusAsync is anonymous-friendly (no auth needed).
    Task<TenantStatusDto?> GetTenantPublicStatusAsync(string slug, CancellationToken ct = default);
    Task<MaintenanceWindowDto> AddMaintenanceWindowAsync(Guid tenantId, CreateMaintenanceWindowRequest request, CancellationToken ct = default);
    Task<bool> RemoveMaintenanceWindowAsync(Guid tenantId, Guid windowId, CancellationToken ct = default);
    Task<bool> ResolveMaintenanceWindowAsync(Guid tenantId, Guid windowId, CancellationToken ct = default);

    // MT-02 — billing surface. GetTenantAsync returns the tenant detail with
    // current PlanTier so the Billing page can render the upgrade matrix.
    // CreateBillingInvoiceAsync calls ControlPlane's PayDunya aggregator and
    // returns the hosted-checkout URL.
    // GetBillingHistoryAsync returns the full invoice history for the portal's
    // "recent payments" table.
    Task<TenantDetailDto?> GetTenantAsync(string slug, CancellationToken ct = default);
    Task<CreateInvoiceResponse?> CreateBillingInvoiceAsync(string slug, CreateBillingInvoiceRequest request, CancellationToken ct = default);
    Task<CreateInvoiceResponse?> CreateFlutterwaveBillingInvoiceAsync(string slug, CreateBillingInvoiceRequest request, CancellationToken ct = default);
    Task<TenantPaymentHistoryDto?> GetBillingHistoryAsync(string slug, CancellationToken ct = default);

    // MT-04 — per-tenant custom SMTP mail relay
    Task<Store.Models.DTOs.Tenant.TenantSmtpConfigDto?> GetSmtpConfigAsync(Guid tenantId, CancellationToken ct = default);
    Task<Store.Models.DTOs.Tenant.TenantSmtpConfigDto> UpdateSmtpConfigAsync(Guid tenantId, Store.Models.DTOs.Tenant.UpdateTenantSmtpRequest request, CancellationToken ct = default);
    Task<Store.Models.DTOs.Tenant.TestSmtpResponse> TestSmtpConfigAsync(Guid tenantId, Store.Models.DTOs.Tenant.TestSmtpRequest request, CancellationToken ct = default);
    Task<bool> ResetSmtpConfigAsync(Guid tenantId, CancellationToken ct = default);
}


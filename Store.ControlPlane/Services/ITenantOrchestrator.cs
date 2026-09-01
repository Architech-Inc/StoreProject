using Store.ControlPlane.Models.DTOs;

namespace Store.ControlPlane.Services;

public interface ITenantOrchestrator
{
    Task<TenantDto> ProvisionTenantAsync(ProvisionTenantRequest request, CancellationToken ct = default);
    Task<TenantDetailDto?> GetTenantDetailsAsync(Guid tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<TenantDto>> GetAllTenantsAsync(CancellationToken ct = default);
    Task<TenantDto?> SuspendTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<TenantDto?> ResumeTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> DeprovisionTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<TenantHealthSummaryDto> GetHealthSummaryAsync(CancellationToken ct = default);
    Task<bool> CheckTenantHealthAsync(Guid tenantId, CancellationToken ct = default);

    // Environment control
    Task<EnvironmentStatusDto?> GetEnvironmentStatusAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> RestartContainerAsync(Guid tenantId, string serviceName, CancellationToken ct = default);
    Task<bool> RestartAllContainersAsync(Guid tenantId, CancellationToken ct = default);

    // Custom Domains
    Task<TenantDomainDto?> GetDomainConfigAsync(Guid tenantId, CancellationToken ct = default);
    Task<TenantDomainDto> SetCustomDomainAsync(Guid tenantId, string domain, CancellationToken ct = default);
    Task<VerifyDomainResponse> VerifyCustomDomainAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> RemoveCustomDomainAsync(Guid tenantId, CancellationToken ct = default);

    // Branch Subdomains
    Task<IReadOnlyList<BranchDto>> GetBranchesAsync(Guid tenantId, CancellationToken ct = default);
    Task<BranchDto> AddBranchAsync(Guid tenantId, CreateBranchRequest request, CancellationToken ct = default);
    Task<VerifyDomainResponse> VerifyBranchAsync(Guid tenantId, Guid branchId, CancellationToken ct = default);
    Task<bool> RemoveBranchAsync(Guid tenantId, Guid branchId, CancellationToken ct = default);
}

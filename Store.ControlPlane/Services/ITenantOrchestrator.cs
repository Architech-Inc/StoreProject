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
}

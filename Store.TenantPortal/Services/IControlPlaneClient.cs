using Store.TenantPortal.Models.DTOs;

namespace Store.TenantPortal.Services;

public interface IControlPlaneClient
{
    Task<SlugCheckDto> CheckSlugAvailabilityAsync(string slug, CancellationToken ct = default);
    Task<PortalAuthDto> RegisterAccountAsync(string email, string fullName, string password, CancellationToken ct = default);
    Task<PortalAuthDto?> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<TenantSummaryDto> ProvisionTenantAsync(ProvisionTenantDto request, CancellationToken ct = default);
    Task<TenantDetailDto?> GetTenantDetailsAsync(Guid tenantId, CancellationToken ct = default);
    Task<bool> CheckTenantHealthAsync(Guid tenantId, CancellationToken ct = default);
    Task LinkAccountToTenantAsync(Guid accountId, Guid tenantId, CancellationToken ct = default);
}

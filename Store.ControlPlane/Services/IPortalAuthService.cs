using Store.ControlPlane.Models.DTOs;

namespace Store.ControlPlane.Services;

public interface IPortalAuthService
{
    Task<PortalAuthResponse> RegisterAsync(RegisterPortalAccountRequest request, CancellationToken ct = default);
    Task<PortalAuthResponse?> LoginAsync(LoginPortalAccountRequest request, CancellationToken ct = default);
    Task<SlugCheckResponse> CheckSlugAvailabilityAsync(string slug, CancellationToken ct = default);
    Task LinkAccountToTenantAsync(Guid accountId, Guid tenantId, CancellationToken ct = default);
}

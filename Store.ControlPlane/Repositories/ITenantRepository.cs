using Store.ControlPlane.Models;

namespace Store.ControlPlane.Repositories;

public interface ITenantRepository
{
    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default);
    Task<Tenant?> GetByIdAsync(Guid tenantId, CancellationToken ct = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
    Task SaveAsync(Tenant tenant, CancellationToken ct = default);
    Task DeleteAsync(Guid tenantId, CancellationToken ct = default);
}

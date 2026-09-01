using Store.ControlPlane.Models;

namespace Store.ControlPlane.Services;

public interface ITraefikConfigWriter
{
    Task WriteTenantRoutingConfigAsync(Tenant tenant, CancellationToken ct = default);
    Task RemoveTenantRoutingConfigAsync(string slug, CancellationToken ct = default);
}

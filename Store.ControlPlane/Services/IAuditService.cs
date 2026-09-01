using Store.ControlPlane.Models.DTOs;

namespace Store.ControlPlane.Services;

public interface IAuditService
{
    Task RecordAuditAsync(Guid tenantId, string actionType, string actorEmail, string details, string? ipAddress = null, CancellationToken ct = default);
    Task<IReadOnlyList<TenantAuditDto>> GetAuditTrailAsync(Guid tenantId, int limit = 50, CancellationToken ct = default);
}

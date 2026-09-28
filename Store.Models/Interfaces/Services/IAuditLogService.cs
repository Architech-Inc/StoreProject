using Store.Models.DTOs.Audit;
using Store.Models.DTOs.Common;

namespace Store.Models.Interfaces.Services;

public interface IAuditLogService
{
    /// <summary>
    /// MT-05 — when <paramref name="tenantId"/> is supplied, metrics are
    /// restricted to that tenant. NULL returns cross-tenant totals (caller
    /// must have cross-tenant authority to use this).
    /// </summary>
    Task<AuditLogMetricsDto> GetMetricsAsync(Guid? tenantId = null, CancellationToken ct = default);
    Task<PagedResult<AuditLogDto>> GetAuditLogsPagedAsync(AuditLogFilterRequest request, CancellationToken ct = default);
    Task<AuditLogDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<AuditLogDto> LogAsync(CreateAuditLogEntryRequest request, CancellationToken ct = default);
    Task<IReadOnlyCollection<AuditLogDto>> GetRecentUserActivityAsync(Guid userId, int limit = 10, CancellationToken ct = default);
    Task<int> PruneLogsOlderThanAsync(DateTime threshold, CancellationToken ct = default);
}

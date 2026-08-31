using Store.Models.DTOs.Audit;
using Store.Models.DTOs.Common;

namespace Store.Models.Interfaces.Services;

public interface IAuditLogService
{
    Task<AuditLogMetricsDto> GetMetricsAsync(CancellationToken ct = default);
    Task<PagedResult<AuditLogDto>> GetAuditLogsPagedAsync(AuditLogFilterRequest request, CancellationToken ct = default);
    Task<AuditLogDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<AuditLogDto> LogAsync(CreateAuditLogEntryRequest request, CancellationToken ct = default);
    Task<IReadOnlyCollection<AuditLogDto>> GetRecentUserActivityAsync(Guid userId, int limit = 10, CancellationToken ct = default);
    Task<int> PruneLogsOlderThanAsync(DateTime threshold, CancellationToken ct = default);
}

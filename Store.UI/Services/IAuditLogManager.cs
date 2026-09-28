using Store.Models.DTOs.Audit;
using Store.Models.DTOs.Common;

namespace StoreUI.Services;

public interface IAuditLogManager
{
    /// <summary>
    /// MT-05 — tenant-scoped metrics. Pass a tenantId to roll up only that
    /// tenant's events; NULL returns cross-tenant totals (system admin).
    /// </summary>
    Task<AuditLogMetricsDto> GetMetricsAsync(Guid? tenantId = null, CancellationToken ct = default);
    Task<PagedResult<AuditLogDto>> GetAuditLogsPagedAsync(AuditLogFilterRequest request, CancellationToken ct = default);
    Task<AuditLogDto?> GetAuditLogByIdAsync(long id, CancellationToken ct = default);
    byte[] ExportCsv(IEnumerable<AuditLogDto> logs);
    byte[] ExportJson(IEnumerable<AuditLogDto> logs);
}

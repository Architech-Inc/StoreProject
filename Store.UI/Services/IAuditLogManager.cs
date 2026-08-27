using Store.Models.DTOs.Audit;
using Store.Models.DTOs.Common;

namespace StoreUI.Services;

public interface IAuditLogManager
{
    Task<AuditLogMetricsDto> GetMetricsAsync(CancellationToken ct = default);
    Task<PagedResult<AuditLogDto>> GetAuditLogsPagedAsync(AuditLogFilterRequest request, CancellationToken ct = default);
    Task<AuditLogDto?> GetAuditLogByIdAsync(long id, CancellationToken ct = default);
    byte[] ExportCsv(IEnumerable<AuditLogDto> logs);
    byte[] ExportJson(IEnumerable<AuditLogDto> logs);
}

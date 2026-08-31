using Store.Models.Entities;

namespace Store.Models.Interfaces.Services;

public interface ICommunicationLogService
{
    Task LogCommunicationAsync(CommunicationLog log, CancellationToken ct = default);
    Task<List<CommunicationLog>> GetLogsAsync(int page = 1, int pageSize = 50, string? channel = null, string? status = null, CancellationToken ct = default);
    Task<long> GetLogsCountAsync(string? channel = null, string? status = null, CancellationToken ct = default);
    Task<int> PruneLogsOlderThanAsync(DateTime threshold, CancellationToken ct = default);
}

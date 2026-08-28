using Store.Models.Entities;

namespace StoreUI.Services;

public interface ICommunicationManager
{
    Task<(List<CommunicationLog> Logs, long TotalCount)> GetLogsPagedAsync(int page = 1, int pageSize = 50, string? channel = null, string? status = null, CancellationToken ct = default);
}

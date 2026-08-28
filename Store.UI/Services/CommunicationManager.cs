using Store.Models.Entities;

namespace StoreUI.Services;

public class CommunicationManager : ICommunicationManager
{
    private readonly IApiCommunicationLogService _logService;

    public CommunicationManager(IApiCommunicationLogService logService)
    {
        _logService = logService;
    }

    public async Task<(List<CommunicationLog> Logs, long TotalCount)> GetLogsPagedAsync(int page = 1, int pageSize = 50, string? channel = null, string? status = null, CancellationToken ct = default)
    {
        return await _logService.GetLogsAsync(page, pageSize, channel, status, ct);
    }
}

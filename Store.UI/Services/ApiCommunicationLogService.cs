using Store.Models.Entities;

namespace StoreUI.Services;

public interface IApiCommunicationLogService
{
    Task<(List<CommunicationLog> Logs, long TotalCount)> GetLogsAsync(int page = 1, int pageSize = 50, string? channel = null, string? status = null, CancellationToken ct = default);
}

public class ApiCommunicationLogService : IApiCommunicationLogService
{
    private readonly IApiClientService _client;

    public ApiCommunicationLogService(IApiClientService client)
    {
        _client = client;
    }

    public async Task<(List<CommunicationLog> Logs, long TotalCount)> GetLogsAsync(int page = 1, int pageSize = 50, string? channel = null, string? status = null, CancellationToken ct = default)
    {
        var query = $"?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(channel)) query += $"&channel={channel}";
        if (!string.IsNullOrWhiteSpace(status)) query += $"&status={status}";

        var result = await _client.GetAsync<CommunicationLogsResponse>($"/api/communicationlogs{query}", ct);
        
        if (result != null && result.Data != null)
        {
            return (result.Data, result.TotalCount);
        }

        return (new List<CommunicationLog>(), 0);
    }
}

public class CommunicationLogsResponse
{
    public List<CommunicationLog> Data { get; set; } = new();
    public long TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

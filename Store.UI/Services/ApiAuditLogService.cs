using Store.Models.DTOs.Audit;
using Store.Models.DTOs.Common;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiAuditLogService : IAuditLogService
{
    private readonly IApiClientService _client;

    public ApiAuditLogService(IApiClientService client)
    {
        _client = client;
    }

    public async Task<AuditLogMetricsDto> GetMetricsAsync(CancellationToken ct = default)
        => await _client.GetAsync<AuditLogMetricsDto>("/api/audit-logs/metrics") ?? new();

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsPagedAsync(AuditLogFilterRequest request, CancellationToken ct = default)
    {
        var qs = new List<string>
        {
            $"page={request.Page}",
            $"pageSize={request.PageSize}"
        };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            qs.Add($"searchTerm={Uri.EscapeDataString(request.SearchTerm)}");

        if (!string.IsNullOrWhiteSpace(request.Category))
            qs.Add($"category={Uri.EscapeDataString(request.Category)}");

        if (!string.IsNullOrWhiteSpace(request.Severity))
            qs.Add($"severity={Uri.EscapeDataString(request.Severity)}");

        if (request.UserId.HasValue && request.UserId.Value != Guid.Empty)
            qs.Add($"userId={request.UserId.Value}");

        if (request.FromDate.HasValue)
            qs.Add($"fromDate={request.FromDate.Value:yyyy-MM-dd}");

        if (request.ToDate.HasValue)
            qs.Add($"toDate={request.ToDate.Value:yyyy-MM-dd}");

        var url = $"/api/audit-logs/paged?{string.Join("&", qs)}";
        return await _client.GetAsync<PagedResult<AuditLogDto>>(url) ?? new();
    }

    public async Task<AuditLogDto?> GetByIdAsync(long id, CancellationToken ct = default)
        => await _client.GetAsync<AuditLogDto>($"/api/audit-logs/{id}");

    public async Task<AuditLogDto> LogAsync(CreateAuditLogEntryRequest request, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<AuditLogDto>("/api/audit-logs", request);
        return result ?? throw new InvalidOperationException("Failed to create audit log entry.");
    }

    public async Task<IReadOnlyCollection<AuditLogDto>> GetRecentUserActivityAsync(Guid userId, int limit = 10, CancellationToken ct = default)
        => await _client.GetAsync<List<AuditLogDto>>($"/api/users/profile/activity") ?? new List<AuditLogDto>();
}

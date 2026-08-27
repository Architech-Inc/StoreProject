using Store.Models.DTOs.Common;
using Store.Models.DTOs.Inventory;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiBatchService : IBatchService
{
    private readonly IApiClientService _client;

    public ApiBatchService(IApiClientService client) => _client = client;

    public async Task<List<BatchDto>> GetAllAsync(Guid? itemId = null, string? expiryStatus = null)
    {
        var qs = new List<string>();
        if (itemId.HasValue) qs.Add($"itemId={itemId.Value}");
        if (!string.IsNullOrWhiteSpace(expiryStatus)) qs.Add($"expiryStatus={Uri.EscapeDataString(expiryStatus)}");
        var query = qs.Count > 0 ? "?" + string.Join("&", qs) : "";
        var result = await _client.GetAsync<List<BatchDto>>($"/api/batches{query}");
        return result ?? new List<BatchDto>();
    }

    public async Task<PagedResult<BatchDto>> GetBatchesPagedAsync(BatchFilterRequest request, CancellationToken ct = default)
    {
        var qs = new List<string>
        {
            $"page={request.Page}",
            $"pageSize={request.PageSize}"
        };
        if (request.ItemId.HasValue) qs.Add($"itemId={request.ItemId.Value}");
        if (!string.IsNullOrWhiteSpace(request.ExpiryStatus)) qs.Add($"expiryStatus={Uri.EscapeDataString(request.ExpiryStatus)}");
        if (!string.IsNullOrWhiteSpace(request.SearchTerm)) qs.Add($"searchTerm={Uri.EscapeDataString(request.SearchTerm)}");
        if (request.FromExpiry.HasValue) qs.Add($"fromExpiry={Uri.EscapeDataString(request.FromExpiry.Value.ToString("O"))}");
        if (request.ToExpiry.HasValue) qs.Add($"toExpiry={Uri.EscapeDataString(request.ToExpiry.Value.ToString("O"))}");

        var query = "?" + string.Join("&", qs);
        var result = await _client.GetAsync<PagedResult<BatchDto>>($"/api/batches/paged{query}", ct);
        return result ?? new PagedResult<BatchDto>();
    }

    public async Task<BatchMetricsDto> GetBatchMetricsAsync(CancellationToken ct = default)
    {
        var result = await _client.GetAsync<BatchMetricsDto>("/api/batches/metrics", ct);
        return result ?? new BatchMetricsDto();
    }

    public async Task<BatchDto?> GetByIdAsync(Guid id)
        => await _client.GetAsync<BatchDto>($"/api/batches/{id}");

    public async Task<BatchDto> CreateAsync(CreateBatchRequest request)
    {
        var result = await _client.PostAsync<BatchDto>("/api/batches", request);
        return result ?? throw new InvalidOperationException("Failed to create batch.");
    }

    public async Task<BatchDto?> UpdateAsync(Guid id, UpdateBatchRequest request)
        => await _client.PutAsync<BatchDto>($"/api/batches/{id}", request);

    public async Task<bool> DeleteAsync(Guid id)
        => await _client.DeleteAsync($"/api/batches/{id}");

    public async Task<bool> WriteOffBatchAsync(WriteOffBatchRequest request, Guid? actingUserId, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<bool>("/api/batches/write-off", request, ct);
        return result;
    }

    public async Task<List<BatchDto>> GetExpiringAsync(int withinDays = 30)
    {
        var result = await _client.GetAsync<List<BatchDto>>($"/api/batches/expiring?withinDays={withinDays}");
        return result ?? new List<BatchDto>();
    }
}

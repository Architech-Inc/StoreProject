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

    public async Task<List<BatchDto>> GetExpiringAsync(int withinDays = 30)
    {
        var result = await _client.GetAsync<List<BatchDto>>($"/api/batches/expiring?withinDays={withinDays}");
        return result ?? new List<BatchDto>();
    }
}

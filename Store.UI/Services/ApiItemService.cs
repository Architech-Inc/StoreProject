using Store.Models.DTOs.Auth;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Items;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiItemService : IItemService
{
    private readonly IApiClientService _client;
    private readonly ILogger<ApiItemService> _logger;

    public ApiItemService(IApiClientService client, ILogger<ApiItemService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<ItemDto?> GetByIdAsync(Guid itemId, CancellationToken ct = default)
    {
        return await _client.GetAsync<ItemDto>($"/api/items/{itemId}", ct);
    }

    public async Task<PagedResult<ItemDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        var qs = $"?page={request.Page}&pageSize={request.PageSize}";
        var result = await _client.GetAsync<PagedResult<ItemDto>>($"/api/items{qs}", ct);
        return result ?? new PagedResult<ItemDto>();
    }

    public async Task<IEnumerable<ItemDto>> GetLowStockAsync(CancellationToken ct = default)
    {
        var result = await _client.GetAsync<IEnumerable<ItemDto>>("/api/items/low-stock", ct);
        return result ?? Enumerable.Empty<ItemDto>();
    }

    public async Task<ItemDto> CreateAsync(CreateItemRequest request, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<ItemDto>("/api/items", request, ct);
        return result ?? throw new InvalidOperationException("Failed to create item");
    }

    public async Task<ItemDto?> UpdateAsync(Guid itemId, UpdateItemRequest request, CancellationToken ct = default)
    {
        return await _client.PutAsync<ItemDto>($"/api/items/{itemId}", request, ct);
    }

    public async Task<bool> AdjustStockAsync(Guid itemId, AdjustStockRequest request, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<bool?>($"/api/items/{itemId}/adjust-stock", request, ct);
        return result.HasValue && result.Value;
    }

    public async Task<bool> DeleteAsync(Guid itemId, CancellationToken ct = default)
    {
        return await _client.DeleteAsync($"/api/items/{itemId}", ct);
    }
}

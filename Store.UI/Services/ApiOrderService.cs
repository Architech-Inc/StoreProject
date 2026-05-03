using Store.Models.DTOs.Common;
using Store.Models.DTOs.Orders;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiOrderService : IOrderService
{
    private readonly IApiClientService _client;
    private readonly ILogger<ApiOrderService> _logger;

    public ApiOrderService(IApiClientService client, ILogger<ApiOrderService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<OrderDto?> GetByIdAsync(Guid orderId, CancellationToken ct = default)
        => await _client.GetAsync<OrderDto>($"/api/orders/{orderId}", ct);

    public async Task<PagedResult<OrderDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        var qs = $"?page={request.Page}&pageSize={request.PageSize}";
        var result = await _client.GetAsync<PagedResult<OrderDto>>($"/api/orders{qs}", ct);
        return result ?? new PagedResult<OrderDto>();
    }

    public async Task<OrderDto> CreateAsync(CreateOrderRequest request, Guid? actingUserId, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<OrderDto>("/api/orders", request, ct);
        return result ?? throw new InvalidOperationException("Failed to create order.");
    }

    public async Task<bool> ReceiveOrderAsync(Guid orderId, CancellationToken ct = default)
        => await _client.PostAsync($"/api/orders/{orderId}/receive", null, ct);

    public async Task<bool> CancelOrderAsync(Guid orderId, CancellationToken ct = default)
        => await _client.PostAsync($"/api/orders/{orderId}/cancel", null, ct);
}

using Store.Models.DTOs.Procurement;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiPurchaseOrderService : IPurchaseOrderService
{
    private readonly IApiClientService _client;

    public ApiPurchaseOrderService(IApiClientService client) => _client = client;

    public async Task<List<PurchaseOrderDto>> GetAllAsync(PurchaseOrderStatus? status = null, Guid? supplierId = null)
    {
        var qs = new List<string>();
        if (status.HasValue) qs.Add($"status={status.Value}");
        if (supplierId.HasValue) qs.Add($"supplierId={supplierId.Value}");
        var query = qs.Count > 0 ? "?" + string.Join("&", qs) : "";
        return await _client.GetAsync<List<PurchaseOrderDto>>($"/api/purchase-orders{query}") ?? new();
    }

    public async Task<PurchaseOrderDto?> GetByIdAsync(int id)
        => await _client.GetAsync<PurchaseOrderDto>($"/api/purchase-orders/{id}");

    public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderRequest request, Guid requestedByUserId)
    {
        var result = await _client.PostAsync<PurchaseOrderDto>("/api/purchase-orders", request);
        return result ?? throw new InvalidOperationException("Failed to create purchase order.");
    }

    public async Task<PurchaseOrderDto?> SubmitAsync(int id, Guid userId)
        => await _client.PostAsync<PurchaseOrderDto>($"/api/purchase-orders/{id}/submit", null);

    public async Task<PurchaseOrderDto?> ApproveAsync(int id, Guid approvedByUserId)
        => await _client.PostAsync<PurchaseOrderDto>($"/api/purchase-orders/{id}/approve", null);

    public async Task<PurchaseOrderDto?> ReceiveAsync(int id, ReceivePurchaseOrderRequest request, Guid receivedByUserId)
        => await _client.PostAsync<PurchaseOrderDto>($"/api/purchase-orders/{id}/receive", request);

    public async Task<PurchaseOrderDto?> CancelAsync(int id, Guid userId)
        => await _client.PostAsync<PurchaseOrderDto>($"/api/purchase-orders/{id}/cancel", null);
}

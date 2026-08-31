using Store.Models.DTOs.Common;
using Store.Models.DTOs.Procurement;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class ApiPurchaseOrderService : IPurchaseOrderService
{
    private readonly IApiClientService _client;

    public ApiPurchaseOrderService(IApiClientService client) => _client = client;

    public async Task<PurchaseOrderMetricsDto> GetPurchaseOrderMetricsAsync(CancellationToken ct = default)
        => await _client.GetAsync<PurchaseOrderMetricsDto>("/api/purchase-orders/metrics") ?? new();

    public async Task<PagedResult<PurchaseOrderDto>> GetPurchaseOrdersPagedAsync(PurchaseOrderFilterRequest request, CancellationToken ct = default)
    {
        var qs = new List<string>
        {
            $"page={request.Page}",
            $"pageSize={request.PageSize}"
        };

        if (!string.IsNullOrWhiteSpace(request.Status))
            qs.Add($"status={Uri.EscapeDataString(request.Status)}");

        if (request.SupplierId.HasValue && request.SupplierId.Value != Guid.Empty)
            qs.Add($"supplierId={request.SupplierId.Value}");

        if (request.BranchId.HasValue && request.BranchId.Value > 0)
            qs.Add($"branchId={request.BranchId.Value}");

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            qs.Add($"searchTerm={Uri.EscapeDataString(request.SearchTerm)}");

        if (request.FromDate.HasValue)
            qs.Add($"fromDate={request.FromDate.Value:yyyy-MM-dd}");

        if (request.ToDate.HasValue)
            qs.Add($"toDate={request.ToDate.Value:yyyy-MM-dd}");

        var url = $"/api/purchase-orders/paged?{string.Join("&", qs)}";
        return await _client.GetAsync<PagedResult<PurchaseOrderDto>>(url) ?? new();
    }

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

    public async Task<AutomatedReorderResultDto> ExecuteAutomatedReorderAsync(Guid? actingUserId = null, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<AutomatedReorderResultDto>("/api/purchase-orders/auto-reorder/trigger", null, ct);
        return result ?? new AutomatedReorderResultDto { Message = "No response from server." };
    }
}

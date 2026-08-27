using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;

namespace StoreUI.Services;

public class InventoryOpsManager : IInventoryOpsManager
{
    private readonly IApiClientService _apiClient;

    public InventoryOpsManager(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<InventoryMetricsDto> GetMetricsAsync(CancellationToken ct = default)
    {
        var result = await _apiClient.GetAsync<InventoryMetricsDto>("/api/inventory/metrics", ct);
        return result ?? new InventoryMetricsDto();
    }

    public async Task<PagedResult<StockMovementDto>> GetMovementsAsync(StockMovementFilterRequest request, CancellationToken ct = default)
    {
        var qs = $"?page={request.Page}&pageSize={request.PageSize}";
        if (request.MovementType.HasValue)
            qs += $"&movementType={(int)request.MovementType.Value}";
        if (request.ItemId.HasValue)
            qs += $"&itemId={request.ItemId.Value}";
        if (request.FromDate.HasValue)
            qs += $"&fromDate={Uri.EscapeDataString(request.FromDate.Value.ToString("O"))}";
        if (request.ToDate.HasValue)
            qs += $"&toDate={Uri.EscapeDataString(request.ToDate.Value.ToString("O"))}";
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            qs += $"&searchTerm={Uri.EscapeDataString(request.SearchTerm.Trim())}";

        var result = await _apiClient.GetAsync<PagedResult<StockMovementDto>>($"/api/inventory/movements{qs}", ct);
        return result ?? new PagedResult<StockMovementDto>();
    }

    public async Task<IReadOnlyList<ReorderSuggestionDto>> GetReorderSuggestionsAsync(CancellationToken ct = default)
    {
        var result = await _apiClient.GetAsync<List<ReorderSuggestionDto>>("/api/inventory/reorder", ct);
        return result ?? new List<ReorderSuggestionDto>();
    }

    public async Task<InventoryOperationResultDto> ReceiveGoodsAsync(GoodsReceiptRequest request, CancellationToken ct = default)
    {
        var result = await _apiClient.PostAsync<InventoryOperationResultDto>("/api/inventory/receive", request, ct);
        return result ?? new InventoryOperationResultDto { Success = false, Message = "No response from inventory service." };
    }

    public async Task<InventoryOperationResultDto> ProcessReturnAsync(StockReturnRequest request, CancellationToken ct = default)
    {
        var result = await _apiClient.PostAsync<InventoryOperationResultDto>("/api/inventory/return", request, ct);
        return result ?? new InventoryOperationResultDto { Success = false, Message = "No response from inventory service." };
    }

    public async Task<InventoryOperationResultDto> AdjustStockAsync(StockAdjustmentAuditRequest request, CancellationToken ct = default)
    {
        var result = await _apiClient.PostAsync<InventoryOperationResultDto>("/api/inventory/adjust", request, ct);
        return result ?? new InventoryOperationResultDto { Success = false, Message = "No response from inventory service." };
    }
}

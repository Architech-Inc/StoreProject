using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;

namespace StoreUI.Services;

public interface IInventoryOpsManager
{
    Task<InventoryMetricsDto> GetMetricsAsync(CancellationToken ct = default);
    Task<PagedResult<StockMovementDto>> GetMovementsAsync(StockMovementFilterRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ReorderSuggestionDto>> GetReorderSuggestionsAsync(CancellationToken ct = default);
    Task<InventoryOperationResultDto> ReceiveGoodsAsync(GoodsReceiptRequest request, CancellationToken ct = default);
    Task<InventoryOperationResultDto> ProcessReturnAsync(StockReturnRequest request, CancellationToken ct = default);
    Task<InventoryOperationResultDto> AdjustStockAsync(StockAdjustmentAuditRequest request, CancellationToken ct = default);
}

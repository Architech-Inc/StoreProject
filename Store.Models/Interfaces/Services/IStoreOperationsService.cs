using Store.Models.DTOs.Operations;
using Store.Models.Enums;

namespace Store.Models.Interfaces.Services;

public interface IStoreOperationsService
{
    Task<IReadOnlyList<StockMovementDto>> GetStockMovementsAsync(int page, int pageSize, StockMovementType? type = null, CancellationToken ct = default);
    Task<InventoryOperationResultDto> ReceiveGoodsAsync(GoodsReceiptRequest request, Guid? actingUserId, CancellationToken ct = default);
    Task<InventoryOperationResultDto> ProcessReturnAsync(StockReturnRequest request, Guid? actingUserId, CancellationToken ct = default);
    Task<InventoryOperationResultDto> AdjustStockAsync(StockAdjustmentAuditRequest request, Guid? actingUserId, CancellationToken ct = default);
    Task<IReadOnlyList<ReorderSuggestionDto>> GetLowStockReorderSuggestionsAsync(CancellationToken ct = default);

    Task<IReadOnlyList<TaxProfileDto>> GetTaxProfilesAsync(CancellationToken ct = default);
    Task<TaxProfileDto> UpsertTaxProfileAsync(UpsertTaxProfileRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<BundleRuleDto>> GetBundleRulesAsync(CancellationToken ct = default);
    Task<BundleRuleDto> UpsertBundleRuleAsync(UpsertBundleRuleRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<SegmentPricingDto>> GetSegmentPricingsAsync(CancellationToken ct = default);
    Task<SegmentPricingDto> UpsertSegmentPricingAsync(UpsertSegmentPricingRequest request, CancellationToken ct = default);
    Task<PricingPreviewDto?> GetPricingPreviewAsync(PricingPreviewRequest request, CancellationToken ct = default);

    Task<CashierShiftDto> OpenShiftAsync(ShiftOpenRequest request, Guid actingUserId, CancellationToken ct = default);
    Task<CashierShiftDto?> GetActiveShiftAsync(Guid actingUserId, CancellationToken ct = default);
    Task<CashierShiftDto?> CloseShiftAsync(ShiftCloseRequest request, Guid actingUserId, CancellationToken ct = default);
    Task<DailyZReportDto> GetDailyZReportAsync(DateTime dateUtc, CancellationToken ct = default);

    Task<IReadOnlyList<RoleMatrixDto>> GetRoleMatrixAsync(CancellationToken ct = default);
    Task<RolePermissionDto> UpdateRolePermissionAsync(UpdateRolePermissionRequest request, CancellationToken ct = default);
}

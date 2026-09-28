using Store.Models.Entities.Inventory;

namespace Store.DbServices.Services;

public interface IDemandForecastingService
{
    Task RunDemandForecastingAsync(CancellationToken ct = default);
    Task<List<RestockRecommendation>> GetPendingRecommendationsAsync(int? branchId = null, CancellationToken ct = default);

    Task<bool> ConvertToStockTransferAsync(Guid recommendationId, Guid requestedByUserId, CancellationToken ct = default);
    Task<bool> ConvertToPurchaseOrderAsync(Guid recommendationId, Guid requestedByUserId, CancellationToken ct = default);
    Task<bool> DismissRecommendationAsync(Guid recommendationId, CancellationToken ct = default);

    /// <summary>
    /// Picks the best supplier for an item, in this order: Item.PreferredSupplierId,
    /// any supplier that has previously supplied the item, else null. The <see cref="SupplierPickResult.Reason"/>
    /// explains the choice so the controller can surface it in error messages.
    /// </summary>
    Task<SupplierPickResult> PickSupplierForItemAsync(Guid itemId, CancellationToken ct = default);

    /// <summary>
    /// Convert every pending Critical recommendation into purchase orders,
    /// grouped by supplier. Recommendations that have no resolvable supplier are
    /// returned in <see cref="BulkOrderResult.SkippedRecommendations"/> so the
    /// UI can prompt the operator to fix the catalog.
    /// </summary>
    Task<BulkOrderResult> BulkOrderCriticalAsync(int? branchId, Guid requestedByUserId, CancellationToken ct = default);
}

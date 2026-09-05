using Store.Models.Entities.Inventory;

namespace Store.DbServices.Services;

public interface IDemandForecastingService
{
    Task RunDemandForecastingAsync(CancellationToken ct = default);
    Task<List<RestockRecommendation>> GetPendingRecommendationsAsync(int? branchId = null, CancellationToken ct = default);
    Task<bool> ConvertToStockTransferAsync(Guid recommendationId, Guid requestedByUserId, CancellationToken ct = default);
    Task<bool> ConvertToPurchaseOrderAsync(Guid recommendationId, Guid requestedByUserId, CancellationToken ct = default);
    Task<bool> DismissRecommendationAsync(Guid recommendationId, CancellationToken ct = default);
}

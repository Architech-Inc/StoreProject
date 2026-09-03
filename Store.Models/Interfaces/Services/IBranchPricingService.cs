namespace Store.Models.Interfaces.Services;

public interface IBranchPricingService
{
    /// <summary>
    /// Resolves the smart effective price for an item at a specific branch:
    /// 1. Checks for active branch-specific discount/promotion.
    /// 2. Checks for explicit branch custom unit price override.
    /// 3. Applies branch regional price multiplier (e.g. 1.08).
    /// 4. Falls back to base catalog item unit price.
    /// </summary>
    Task<decimal> ResolveUnitPriceAsync(Guid itemId, int branchId, CancellationToken ct = default);

    /// <summary>
    /// Batch resolves effective unit prices for multiple items at a branch.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, decimal>> ResolveBatchUnitPricesAsync(IEnumerable<Guid> itemIds, int branchId, CancellationToken ct = default);

    /// <summary>
    /// Validates if a coupon or discount is authorized for use at the given branch.
    /// </summary>
    Task<bool> IsDiscountValidForBranchAsync(int discountId, int branchId, CancellationToken ct = default);
}

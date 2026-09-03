using Microsoft.EntityFrameworkCore;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class BranchPricingService : IBranchPricingService
{
    private readonly IUnitOfWork _uow;

    public BranchPricingService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<decimal> ResolveUnitPriceAsync(Guid itemId, int branchId, CancellationToken ct = default)
    {
        var item = await _uow.Repository<Item>().Query().AsNoTracking()
            .FirstOrDefaultAsync(i => i.ItemId == itemId, ct);
        if (item is null) return 0m;

        var branch = await _uow.Repository<Branch>().Query().AsNoTracking()
            .FirstOrDefaultAsync(b => b.BranchId == branchId, ct);
        var multiplier = branch?.PriceMultiplier ?? 1.0m;

        // Tier 1: Check branch-specific custom price override
        var branchStock = await _uow.Repository<BranchItemStock>().Query().AsNoTracking()
            .FirstOrDefaultAsync(s => s.BranchId == branchId && s.ItemId == itemId, ct);
        if (branchStock?.CustomUnitPrice.HasValue == true)
        {
            return branchStock.CustomUnitPrice.Value;
        }

        // Tier 2: Check regional price multiplier (if not 1.0)
        if (multiplier != 1.0m)
        {
            return Math.Round(item.UnitPrice * multiplier, 2);
        }

        // Tier 3: Base catalog price
        return item.UnitPrice;
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> ResolveBatchUnitPricesAsync(IEnumerable<Guid> itemIds, int branchId, CancellationToken ct = default)
    {
        var idList = itemIds.Distinct().ToList();
        if (idList.Count == 0) return new Dictionary<Guid, decimal>();

        var branch = await _uow.Repository<Branch>().Query().AsNoTracking()
            .FirstOrDefaultAsync(b => b.BranchId == branchId, ct);
        var multiplier = branch?.PriceMultiplier ?? 1.0m;

        var items = await _uow.Repository<Item>().Query().AsNoTracking()
            .Where(i => idList.Contains(i.ItemId))
            .ToDictionaryAsync(i => i.ItemId, i => i.UnitPrice, ct);

        var overrides = await _uow.Repository<BranchItemStock>().Query().AsNoTracking()
            .Where(s => s.BranchId == branchId && idList.Contains(s.ItemId) && s.CustomUnitPrice.HasValue)
            .ToDictionaryAsync(s => s.ItemId, s => s.CustomUnitPrice!.Value, ct);

        var result = new Dictionary<Guid, decimal>();
        foreach (var (itemId, basePrice) in items)
        {
            if (overrides.TryGetValue(itemId, out var customPrice))
            {
                result[itemId] = customPrice;
            }
            else if (multiplier != 1.0m)
            {
                result[itemId] = Math.Round(basePrice * multiplier, 2);
            }
            else
            {
                result[itemId] = basePrice;
            }
        }

        return result;
    }

    public async Task<bool> IsDiscountValidForBranchAsync(int discountId, int branchId, CancellationToken ct = default)
    {
        var discount = await _uow.Repository<Discount>().Query().AsNoTracking()
            .Include(d => d.DiscountBranches)
            .FirstOrDefaultAsync(d => d.DiscountId == discountId, ct);

        if (discount is null || !discount.IsActive) return false;

        // If scoped to all branches, valid everywhere
        if (discount.Scope == BranchScope.AllBranches) return true;

        // Otherwise must match one of the selected branches
        return discount.DiscountBranches.Any(b => b.BranchId == branchId);
    }
}

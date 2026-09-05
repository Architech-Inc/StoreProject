using Microsoft.EntityFrameworkCore;
using Store.DbServices.Context;
using Store.Models.Entities.Inventory;
using Store.Models.Enums;

namespace Store.DbServices.Services;

public class DemandForecastingService : IDemandForecastingService
{
    private readonly StoreDbContext _context;

    public DemandForecastingService(StoreDbContext context)
    {
        _context = context;
    }

    public async Task RunDemandForecastingAsync(CancellationToken ct = default)
    {
        // Calculate velocity based on last 30 days
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var branches = await _context.Branches
            .Where(b => b.IsActive)
            .ToListAsync(ct);

        foreach (var branch in branches)
        {
            var branchStocks = await _context.BranchItemStocks
                .Include(bs => bs.Item)
                .Where(bs => bs.BranchId == branch.BranchId)
                .ToListAsync(ct);

            foreach (var stock in branchStocks)
            {
                // Calculate total quantity sold for this item in this branch in the last 30 days
                var salesCount = await _context.Sales
                    .Include(s => s.Invoice)
                    .Where(s => s.Invoice.BranchId == branch.BranchId &&
                                s.ItemId == stock.ItemId &&
                                s.DateCreated >= thirtyDaysAgo)
                    .SumAsync(s => (int?)s.Quantity, ct) ?? 0;

                var dailyVelocity = salesCount / 30.0;
                var leadTime = stock.LeadTimeDays ?? 7; // Default 7 days
                var safetyStock = stock.ReorderLevel ?? (int)(dailyVelocity * leadTime * 0.5); // Default 50% safety margin if not configured

                var reorderPoint = (dailyVelocity * leadTime) + safetyStock;

                if (stock.InStock <= reorderPoint && dailyVelocity > 0)
                {
                    var recommendedQty = stock.ReorderQuantity ?? Math.Max((int)(dailyVelocity * 30), 10); // Order a month's worth by default

                    // Create recommendation
                    var existingRecommendation = await _context.RestockRecommendations
                        .FirstOrDefaultAsync(r => r.BranchId == branch.BranchId && r.ItemId == stock.ItemId && r.Status == RestockRecommendationStatus.Pending, ct);

                    if (existingRecommendation == null)
                    {
                        var recommendation = new RestockRecommendation
                        {
                            BranchId = branch.BranchId,
                            ItemId = stock.ItemId,
                            RecommendedQuantity = recommendedQty,
                            Reason = $"In stock ({stock.InStock}) <= Reorder Point ({reorderPoint:F1}). Velocity: {dailyVelocity:F1}/day."
                        };
                        _context.RestockRecommendations.Add(recommendation);
                    }
                }
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<RestockRecommendation>> GetPendingRecommendationsAsync(int? branchId = null, CancellationToken ct = default)
    {
        var query = _context.RestockRecommendations
            .Include(r => r.Branch)
            .Include(r => r.Item)
            .Where(r => r.Status == RestockRecommendationStatus.Pending);

        if (branchId.HasValue)
        {
            query = query.Where(r => r.BranchId == branchId.Value);
        }

        return await query.ToListAsync(ct);
    }

    public async Task<bool> ConvertToStockTransferAsync(Guid recommendationId, Guid requestedByUserId, CancellationToken ct = default)
    {
        var recommendation = await _context.RestockRecommendations
            .Include(r => r.Branch)
            .Include(r => r.Item)
            .FirstOrDefaultAsync(r => r.RecommendationId == recommendationId, ct);

        if (recommendation == null || recommendation.Status != RestockRecommendationStatus.Pending)
            return false;

        var targetBranch = recommendation.Branch;
        
        // Orchestration logic: Check if branch has a supplying warehouse, and if it's not empty
        if (!targetBranch.SupplyingWarehouseId.HasValue)
        {
            // Fails if there's no configured internal warehouse
            return false;
        }

        var warehouseId = targetBranch.SupplyingWarehouseId.Value;

        // Create Stock Transfer
        var transfer = new Store.Models.Entities.StockTransfer
        {
            FromBranchId = warehouseId,
            ToBranchId = targetBranch.BranchId,
            RequestedByUserId = requestedByUserId,
            Status = Store.Models.Enums.StockTransferStatus.Requested,
            Notes = $"Generated from Restock Recommendation (Velocity: {recommendation.Reason})"
        };

        transfer.Items.Add(new Store.Models.Entities.StockTransferItem
        {
            ItemId = recommendation.ItemId,
            RequestedQuantity = recommendation.RecommendedQuantity
        });

        _context.StockTransfers.Add(transfer);
        
        recommendation.Status = RestockRecommendationStatus.ConvertedToTransfer;
        recommendation.GeneratedStockTransfer = transfer;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ConvertToPurchaseOrderAsync(Guid recommendationId, Guid requestedByUserId, CancellationToken ct = default)
    {
        var recommendation = await _context.RestockRecommendations
            .Include(r => r.Item)
            .FirstOrDefaultAsync(r => r.RecommendationId == recommendationId, ct);

        if (recommendation == null || recommendation.Status != RestockRecommendationStatus.Pending)
            return false;

        // Try to find a supplier for the item
        // Assuming Item has a DefaultSupplierId or we pick the first one that supplies it
        // Or we just draft a PO with a placeholder supplier if none exists (for demo, pick first supplier)
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(ct);
        if (supplier == null) return false;

        var po = new Store.Models.Entities.PurchaseOrder
        {
            SupplierId = supplier.SupplierId,
            BranchId = recommendation.BranchId, // Could also point to Warehouse depending on policy
            Status = Store.Models.Enums.PurchaseOrderStatus.Draft,
            Notes = $"Generated from Restock Recommendation (Velocity: {recommendation.Reason})"
        };

        po.Items.Add(new Store.Models.Entities.PurchaseOrderItem
        {
            ItemId = recommendation.ItemId,
            OrderedQuantity = recommendation.RecommendedQuantity,
            UnitCost = recommendation.Item.UnitPrice // In reality, we'd use CustomCostPrice
        });

        _context.PurchaseOrders.Add(po);

        recommendation.Status = RestockRecommendationStatus.ConvertedToPurchaseOrder;
        recommendation.GeneratedPurchaseOrder = po;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DismissRecommendationAsync(Guid recommendationId, CancellationToken ct = default)
    {
        var recommendation = await _context.RestockRecommendations.FindAsync(new object[] { recommendationId }, ct);
        if (recommendation == null || recommendation.Status != RestockRecommendationStatus.Pending)
            return false;

        recommendation.Status = RestockRecommendationStatus.Dismissed;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Store.DbServices.Context;
using Store.Models.DTOs.Notifications;
using Store.Models.Entities.Inventory;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class DemandForecastingService : IDemandForecastingService
{
    private readonly StoreDbContext _context;
    private readonly IRealTimeNotificationService _notifier;
    private readonly ILogger<DemandForecastingService> _logger;

    public DemandForecastingService(
        StoreDbContext context,
        IRealTimeNotificationService notifier,
        ILogger<DemandForecastingService> logger)
    {
        _context = context;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task RunDemandForecastingAsync(CancellationToken ct = default)
    {
        // Calculate velocity based on last 30 days
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var branches = await _context.Branches
            .Where(b => b.IsActive)
            .ToListAsync(ct);

        var newRecommendations = new List<RestockRecommendation>();

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
                var leadTime = stock.LeadTimeDays ?? 7;
                var safetyStock = stock.ReorderLevel ?? (int)(dailyVelocity * leadTime * 0.5);

                var reorderPoint = (dailyVelocity * leadTime) + safetyStock;

                if (stock.InStock <= reorderPoint && dailyVelocity > 0)
                {
                    var recommendedQty = stock.ReorderQuantity ?? Math.Max((int)(dailyVelocity * 30), 10);

                    // Skip if there's already a pending recommendation for this branch+item.
                    var existingRecommendation = await _context.RestockRecommendations
                        .FirstOrDefaultAsync(r => r.BranchId == branch.BranchId &&
                                                   r.ItemId == stock.ItemId &&
                                                   r.Status == RestockRecommendationStatus.Pending, ct);

                    if (existingRecommendation != null) continue;

                    var recommendation = new RestockRecommendation
                    {
                        BranchId = branch.BranchId,
                        ItemId = stock.ItemId,
                        RecommendedQuantity = recommendedQty,
                        Reason = $"In stock ({stock.InStock}) <= Reorder Point ({reorderPoint:F1}). Velocity: {dailyVelocity:F1}/day."
                    };
                    _context.RestockRecommendations.Add(recommendation);
                    newRecommendations.Add(recommendation);
                }
            }
        }

        await _context.SaveChangesAsync(ct);

        // Broadcast each new recommendation via SignalR so any connected manager/admin
        // sees the row appear on the restock page without a manual refresh.
        foreach (var rec in newRecommendations)
        {
            var branch = await _context.Branches.FindAsync(new object[] { rec.BranchId }, ct);
            var stock = await _context.BranchItemStocks
                .FirstOrDefaultAsync(bs => bs.BranchId == rec.BranchId && bs.ItemId == rec.ItemId, ct);
            var item = await _context.Items.FindAsync(new object[] { rec.ItemId }, ct);

            var currentStock = stock?.InStock ?? 0;
            var cost = item?.CostPrice ?? item?.UnitPrice ?? 0;
            var daysOfStock = ExtractDaysOfStock(rec.Reason, currentStock);
            var severity = ComputeSeverity(currentStock, daysOfStock);

            var dto = new RestockRecommendationNotificationDto
            {
                RecommendationId = rec.RecommendationId,
                BranchId = rec.BranchId,
                BranchName = branch?.Name ?? string.Empty,
                ItemId = rec.ItemId,
                ItemName = item?.Name ?? string.Empty,
                ItemBarcode = item?.Barcode,
                RecommendedQuantity = rec.RecommendedQuantity,
                Severity = severity,
                CurrentStock = currentStock,
                DaysOfStock = daysOfStock,
                ProjectedValue = cost * rec.RecommendedQuantity,
                Reason = rec.Reason,
                DateCreated = rec.DateCreated
            };

            try
            {
                await _notifier.NotifyRestockRecommendationAsync(dto, ct);
            }
            catch (Exception ex)
            {
                // A failed broadcast must not break the forecasting cycle.
                _logger.LogWarning(ex, "Failed to broadcast restock recommendation {Id}", rec.RecommendationId);
            }
        }
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

        if (!targetBranch.SupplyingWarehouseId.HasValue)
            return false;

        var warehouseId = targetBranch.SupplyingWarehouseId.Value;

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

    /// <summary>
    /// Convert a single recommendation into a Purchase Order, picking the supplier
    /// in this order:
    ///   1. <c>Item.PreferredSupplierId</c> (set per-item by the catalog team)
    ///   2. Any supplier that has at least one <c>ItemsOrder</c> for this item
    ///   3. null — and the controller surfaces the actionable error message
    /// </summary>
    public async Task<SupplierPickResult> PickSupplierForItemAsync(Guid itemId, CancellationToken ct = default)
    {
        var item = await _context.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.ItemId == itemId, ct);

        if (item is null)
            return SupplierPickResult.Missing("Item not found.");

        if (item.PreferredSupplierId.HasValue)
        {
            var preferred = await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SupplierId == item.PreferredSupplierId.Value, ct);

            if (preferred is not null && !preferred.IsDeleted)
                return SupplierPickResult.Picked(preferred.SupplierId,
                    $"Preferred supplier '{preferred.Name}' (set on item).");

            return SupplierPickResult.Missing(
                $"Item has PreferredSupplierId = {item.PreferredSupplierId} but that supplier is missing or deleted. " +
                "Clear the preferred-supplier on the item or restore the supplier.");
        }

        // Fallback: any supplier that has previously supplied this item (via OrderItem → ItemsOrder).
        var knownSupplierId = await _context.OrderItems
            .AsNoTracking()
            .Where(oi => oi.ItemId == itemId
                         && oi.ItemsOrder != null
                         && oi.ItemsOrder.SupplierId.HasValue
                         && !oi.ItemsOrder.IsDeleted)
            .Select(oi => oi.ItemsOrder!.SupplierId!.Value)
            .FirstOrDefaultAsync(ct);

        if (knownSupplierId != Guid.Empty)
        {
            var supplier = await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SupplierId == knownSupplierId, ct);
            if (supplier is not null && !supplier.IsDeleted)
                return SupplierPickResult.Picked(supplier.SupplierId,
                    $"Supplier '{supplier.Name}' inferred from prior purchase history.");
        }

        return SupplierPickResult.Missing(
            "No preferred supplier is set on the item and no supplier has previously " +
            "supplied this item. Set Item.PreferredSupplierId in the catalog first.");
    }

    public async Task<bool> ConvertToPurchaseOrderAsync(Guid recommendationId, Guid requestedByUserId, CancellationToken ct = default)
    {
        var recommendation = await _context.RestockRecommendations
            .Include(r => r.Item)
            .FirstOrDefaultAsync(r => r.RecommendationId == recommendationId, ct);

        if (recommendation == null || recommendation.Status != RestockRecommendationStatus.Pending)
            return false;

        var pick = await PickSupplierForItemAsync(recommendation.ItemId, ct);
        if (pick.SupplierId is null) return false;

        var po = new Store.Models.Entities.PurchaseOrder
        {
            SupplierId = pick.SupplierId.Value,
            BranchId = recommendation.BranchId,
            Status = Store.Models.Enums.PurchaseOrderStatus.Draft,
            Notes = $"Generated from Restock Recommendation. Supplier chosen: {pick.Reason} " +
                    $"(Velocity: {recommendation.Reason})"
        };

        po.Items.Add(new Store.Models.Entities.PurchaseOrderItem
        {
            ItemId = recommendation.ItemId,
            OrderedQuantity = recommendation.RecommendedQuantity,
            UnitCost = recommendation.Item.CostPrice ?? recommendation.Item.UnitPrice
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

    // ─── Bulk-order all critical ─────────────────────────────────────────────

    /// <summary>
    /// Convert every pending Critical recommendation into purchase orders, grouped
    /// by supplier (using the same preferred-supplier lookup). Returns the list
    /// of created POs and the list of recommendations that couldn't be assigned
    /// a supplier (so the UI can surface "set PreferredSupplierId on these items").
    /// </summary>
    public async Task<BulkOrderResult> BulkOrderCriticalAsync(int? branchId, Guid requestedByUserId, CancellationToken ct = default)
    {
        var query = _context.RestockRecommendations
            .Include(r => r.Item)
            .Include(r => r.Branch)
            .Where(r => r.Status == RestockRecommendationStatus.Pending);

        if (branchId.HasValue)
        {
            query = query.Where(r => r.BranchId == branchId.Value);
        }

        var pending = await query.ToListAsync(ct);
        if (pending.Count == 0)
        {
            return new BulkOrderResult(
                CreatedPurchaseOrders: Array.Empty<BulkOrderPurchaseOrderSummary>(),
                SkippedRecommendations: Array.Empty<BulkOrderSkippedRecommendation>(),
                TotalProcessed: 0);
        }

        // Resolve severity for each row using the same logic the controller uses,
        // so "Critical" matches regardless of who computes it.
        var branchStocks = await _context.BranchItemStocks
            .Where(bs => pending.Select(p => p.BranchId).Distinct().Contains(bs.BranchId) &&
                         pending.Select(p => p.ItemId).Distinct().Contains(bs.ItemId))
            .ToListAsync(ct);
        var stockLookup = branchStocks
            .GroupBy(bs => (bs.BranchId, bs.ItemId))
            .ToDictionary(g => g.Key, g => g.First());

        var criticalRows = new List<RestockRecommendation>();
        var skipped = new List<BulkOrderSkippedRecommendation>();
        var created = new List<BulkOrderPurchaseOrderSummary>();

        foreach (var rec in pending)
        {
            stockLookup.TryGetValue((rec.BranchId, rec.ItemId), out var stock);
            var currentStock = stock?.InStock ?? 0;
            var daysOfStock = ExtractDaysOfStock(rec.Reason, currentStock);
            var severity = ComputeSeverity(currentStock, daysOfStock);
            if (severity != "Critical")
            {
                continue;
            }

            var pick = await PickSupplierForItemAsync(rec.ItemId, ct);
            if (pick.SupplierId is null)
            {
                skipped.Add(new BulkOrderSkippedRecommendation(
                    RecommendationId: rec.RecommendationId,
                    ItemName: rec.Item?.Name ?? string.Empty,
                    Reason: pick.Reason ?? "Could not pick a supplier."));
                continue;
            }

            criticalRows.Add(rec);
        }

        // Group by supplier and create one PO per supplier.
        var bySupplier = criticalRows.GroupBy(r =>
        {
            // PickSupplierForItemAsync was already called above; reuse the result by
            // re-querying (cheap because the candidate set is small). For better
            // performance we cache the picks per-item.
            return r.ItemId;
        });

        var picksCache = new Dictionary<Guid, SupplierPickResult>();
        foreach (var rec in criticalRows)
        {
            if (picksCache.ContainsKey(rec.ItemId)) continue;
            picksCache[rec.ItemId] = await PickSupplierForItemAsync(rec.ItemId, ct);
        }

        var grouped = criticalRows
            .Where(r => picksCache.TryGetValue(r.ItemId, out var p) && p.SupplierId.HasValue)
            .GroupBy(r => picksCache[r.ItemId].SupplierId!.Value);

        foreach (var group in grouped)
        {
            var first = group.First();
            var supplierId = group.Key;
            var supplierPickReason = picksCache[first.ItemId].Reason ?? string.Empty;

            var po = new Store.Models.Entities.PurchaseOrder
            {
                SupplierId = supplierId,
                BranchId = first.BranchId,
                Status = Store.Models.Enums.PurchaseOrderStatus.Draft,
                Notes = $"Bulk generated from Restock Recommendations ({group.Count()} item(s)). " +
                        $"Supplier picked: {supplierPickReason}"
            };

            foreach (var rec in group)
            {
                po.Items.Add(new Store.Models.Entities.PurchaseOrderItem
                {
                    ItemId = rec.ItemId,
                    OrderedQuantity = rec.RecommendedQuantity,
                    UnitCost = (rec.Item?.CostPrice ?? rec.Item?.UnitPrice ?? 0)
                });

                rec.Status = RestockRecommendationStatus.ConvertedToPurchaseOrder;
                rec.GeneratedPurchaseOrder = po;
            }

            _context.PurchaseOrders.Add(po);
            created.Add(new BulkOrderPurchaseOrderSummary(
                PurchaseOrderId: 0, // filled by EF SaveChanges
                SupplierId: supplierId,
                BranchId: first.BranchId,
                ItemCount: group.Count(),
                TotalQuantity: group.Sum(r => r.RecommendedQuantity)));
        }

        await _context.SaveChangesAsync(ct);

        // Fill in the EF-generated PO ids.
        var summaries = new List<BulkOrderPurchaseOrderSummary>(created.Count);
        foreach (var sum in created)
        {
            // Find the freshly-created PO by Notes (which contains the count).
            var matching = await _context.PurchaseOrders
                .Where(p => p.SupplierId == sum.SupplierId
                            && p.BranchId == sum.BranchId
                            && p.Notes != null
                            && p.Notes.Contains($"({sum.ItemCount} item(s))"))
                .OrderByDescending(p => p.PurchaseOrderId)
                .FirstOrDefaultAsync(ct);
            summaries.Add(matching is null
                ? sum
                : sum with { PurchaseOrderId = matching.PurchaseOrderId });
        }

        return new BulkOrderResult(
            CreatedPurchaseOrders: summaries,
            SkippedRecommendations: skipped,
            TotalProcessed: criticalRows.Count);
    }

    // ─── Severity helpers (also used by RestockController) ───────────────────

    internal static double? ExtractDaysOfStock(string reason, int currentStock)
    {
        if (string.IsNullOrWhiteSpace(reason)) return null;

        var marker = "Velocity: ";
        var idx = reason.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return null;
        var tail = reason[(idx + marker.Length)..];
        var slash = tail.IndexOf('/');
        if (slash <= 0) return null;
        var numberText = tail[..slash].Trim();
        if (!double.TryParse(numberText, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var velocity) || velocity <= 0)
            return null;

        return Math.Round(currentStock / velocity, 1);
    }

    internal static string ComputeSeverity(int currentStock, double? daysOfStock)
    {
        if (currentStock <= 0) return "Critical";
        if (daysOfStock.HasValue && daysOfStock.Value < 3) return "Critical";
        if (daysOfStock.HasValue && daysOfStock.Value < 7) return "High";
        return "Medium";
    }
}

// ─── Local DTOs used by the service ─────────────────────────────────────────

public record SupplierPickResult(Guid? SupplierId, string? Reason)
{
    public bool HasSupplier => SupplierId.HasValue;
    public static SupplierPickResult Picked(Guid id, string reason) => new(id, reason);
    public static SupplierPickResult Missing(string reason) => new(null, reason);
}

public record BulkOrderPurchaseOrderSummary(
    int PurchaseOrderId,
    Guid SupplierId,
    int BranchId,
    int ItemCount,
    int TotalQuantity);

public record BulkOrderSkippedRecommendation(
    Guid RecommendationId,
    string ItemName,
    string Reason);

public record BulkOrderResult(
    IReadOnlyList<BulkOrderPurchaseOrderSummary> CreatedPurchaseOrders,
    IReadOnlyList<BulkOrderSkippedRecommendation> SkippedRecommendations,
    int TotalProcessed);

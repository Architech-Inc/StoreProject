using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Store.DbServices.Context;
using Store.DbServices.Services;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;
using Store.Models.Enums;

namespace Store.API.Controllers;

/// <summary>
/// Restock-recommendation surface. Powers the "Restock & Warehouse Orchestration"
/// page in <c>Store.UI</c>: lists velocity-based suggestions, lets a manager
/// convert each into a stock transfer or purchase order, dismiss irrelevant
/// ones, manually trigger the AI forecast cycle, or bulk-order every Critical
/// recommendation as multi-item POs.
/// </summary>
[ApiController]
[Route("api/Restock")]
[Authorize]
public class RestockController : ControllerBase
{
    private readonly IDemandForecastingService _forecasting;
    private readonly StoreDbContext _db;
    private readonly ILogger<RestockController> _logger;

    public RestockController(
        IDemandForecastingService forecasting,
        StoreDbContext db,
        ILogger<RestockController> logger)
    {
        _forecasting = forecasting;
        _db = db;
        _logger = logger;
    }

    // ─── List pending ───────────────────────────────────────────────────────

    [HttpGet("pending")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetPending(
        [FromQuery] int? branchId,
        CancellationToken ct)
    {
        var rows = await _forecasting.GetPendingRecommendationsAsync(branchId, ct);

        var itemIds = rows.Select(r => r.ItemId).Distinct().ToList();
        var branchIds = rows.Select(r => r.BranchId).Distinct().ToList();

        var branchStocks = await _db.BranchItemStocks
            .Where(bs => branchIds.Contains(bs.BranchId) && itemIds.Contains(bs.ItemId))
            .ToListAsync(ct);

        var stockLookup = branchStocks
            .GroupBy(bs => (bs.BranchId, bs.ItemId))
            .ToDictionary(g => g.Key, g => g.First());

        var dtos = rows.Select(r =>
        {
            stockLookup.TryGetValue((r.BranchId, r.ItemId), out var bs);
            var inStock = bs?.InStock ?? 0;
            var cost = r.Item.CostPrice ?? r.Item.UnitPrice;
            var projected = cost * r.RecommendedQuantity;
            var daysOfStock = ExtractDaysOfStock(r.Reason, inStock);

            return new RestockRecommendationDto
            {
                RecommendationId = r.RecommendationId,
                BranchId = r.BranchId,
                BranchName = r.Branch?.Name ?? string.Empty,
                ItemId = r.ItemId,
                ItemName = r.Item?.Name ?? string.Empty,
                ItemBarcode = r.Item?.Barcode,
                ItemThumbnailUrl = r.Item?.ThumbnailUrl,
                RecommendedQuantity = r.RecommendedQuantity,
                Reason = r.Reason,
                DaysOfStock = daysOfStock,
                CurrentStock = inStock,
                ReorderLevel = bs?.ReorderLevel ?? r.Item?.ReorderLevel,
                ProjectedValue = projected,
                Severity = ComputeSeverity(inStock, daysOfStock),
                DateCreated = r.DateCreated,
                Status = r.Status.ToString()
            };
        }).ToList();

        return Ok(ApiResponse<IReadOnlyList<RestockRecommendationDto>>.Ok(dtos));
    }

    [HttpGet("summary")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetSummary(CancellationToken ct)
    {
        var rows = await _forecasting.GetPendingRecommendationsAsync(branchId: null, ct);

        var itemIds = rows.Select(r => r.ItemId).Distinct().ToList();
        var branchIds = rows.Select(r => r.BranchId).Distinct().ToList();
        var branchStocks = await _db.BranchItemStocks
            .Where(bs => branchIds.Contains(bs.BranchId) && itemIds.Contains(bs.ItemId))
            .ToListAsync(ct);
        var stockLookup = branchStocks
            .GroupBy(bs => (bs.BranchId, bs.ItemId))
            .ToDictionary(g => g.Key, g => g.First());

        var criticalCount = 0;
        decimal projectedValue = 0;
        foreach (var rec in rows)
        {
            stockLookup.TryGetValue((rec.BranchId, rec.ItemId), out var bs);
            var inStock = bs?.InStock ?? 0;
            var daysOfStock = ExtractDaysOfStock(rec.Reason, inStock);
            if (ComputeSeverity(inStock, daysOfStock) == "Critical") criticalCount++;
            var cost = rec.Item.CostPrice ?? rec.Item.UnitPrice;
            projectedValue += cost * rec.RecommendedQuantity;
        }

        var summary = new RestockSummaryDto
        {
            PendingCount = rows.Count,
            CriticalCount = criticalCount,
            ProjectedValue = projectedValue,
            Currency = "XAF"
        };

        return Ok(ApiResponse<RestockSummaryDto>.Ok(summary));
    }

    // ─── Convert / Dismiss ──────────────────────────────────────────────────

    [HttpPost("{id:guid}/convert-to-transfer")]
    [Authorize(Policy = PermissionKeys.PurchaseOrderWrite)]
    public async Task<IActionResult> ConvertToTransfer(Guid id, CancellationToken ct)
    {
        var userId = CurrentUserId();
        if (userId is null) return Unauthorized(ApiResponse<object>.Fail("Missing user identity."));

        var ok = await _forecasting.ConvertToStockTransferAsync(id, userId.Value, ct);
        return Ok(ToResult(ok, id, "transfer",
            ok ? "Stock transfer request generated successfully." :
                "Could not generate a transfer: ensure the branch has a configured supplying warehouse."));
    }

    [HttpPost("{id:guid}/convert-to-po")]
    [Authorize(Policy = PermissionKeys.PurchaseOrderWrite)]
    public async Task<IActionResult> ConvertToPurchaseOrder(Guid id, CancellationToken ct)
    {
        var userId = CurrentUserId();
        if (userId is null) return Unauthorized(ApiResponse<object>.Fail("Missing user identity."));

        // Surface the supplier-pick reason on failure so the UI doesn't get a
        // vague "Failed to generate purchase order." message.
        var rec = await _db.RestockRecommendations
            .Include(r => r.Item)
            .FirstOrDefaultAsync(r => r.RecommendationId == id, ct);
        if (rec is null)
        {
            return Ok(ToResult(false, id, "purchase-order", "Recommendation not found."));
        }

        var pick = await _forecasting.PickSupplierForItemAsync(rec.ItemId, ct);
        var ok = await _forecasting.ConvertToPurchaseOrderAsync(id, userId.Value, ct);
        return Ok(ToResult(ok, id, "purchase-order",
            ok ? $"Purchase order generated against supplier. Reason: {pick.Reason}" :
                pick.Reason ?? "Failed to generate purchase order."));
    }

    [HttpPost("{id:guid}/dismiss")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> Dismiss(Guid id, CancellationToken ct)
    {
        var ok = await _forecasting.DismissRecommendationAsync(id, ct);
        return Ok(ToResult(ok, id, "dismiss",
            ok ? "Recommendation dismissed." : "Could not dismiss: recommendation already actioned or not found."));
    }

    // ─── Bulk-order all Critical ─────────────────────────────────────────────

    /// <summary>
    /// Convert every pending Critical recommendation into purchase orders,
    /// grouped by supplier. Returns the list of created POs and the list of
    /// recommendations that had no resolvable supplier.
    /// </summary>
    [HttpPost("bulk-order-critical")]
    [Authorize(Policy = PermissionKeys.PurchaseOrderWrite)]
    public async Task<IActionResult> BulkOrderCritical(
        [FromQuery] int? branchId,
        CancellationToken ct)
    {
        var userId = CurrentUserId();
        if (userId is null) return Unauthorized(ApiResponse<object>.Fail("Missing user identity."));

        var result = await _forecasting.BulkOrderCriticalAsync(branchId, userId.Value, ct);

        // Hydrate supplier + branch names for the UI.
        var supplierIds = result.CreatedPurchaseOrders.Select(p => p.SupplierId).Distinct().ToList();
        var branchIds = result.CreatedPurchaseOrders.Select(p => p.BranchId).Distinct().ToList();
        var suppliers = await _db.Suppliers.Where(s => supplierIds.Contains(s.SupplierId)).ToDictionaryAsync(s => s.SupplierId, ct);
        var branches = await _db.Branches.Where(b => branchIds.Contains(b.BranchId)).ToDictionaryAsync(b => b.BranchId, ct);

        var pos = result.CreatedPurchaseOrders.Select(p => new BulkOrderPurchaseOrderDto
        {
            PurchaseOrderId = p.PurchaseOrderId,
            SupplierId = p.SupplierId,
            SupplierName = suppliers.TryGetValue(p.SupplierId, out var s) ? s.Name : null,
            BranchId = p.BranchId,
            BranchName = branches.TryGetValue(p.BranchId, out var b) ? b.Name : null,
            ItemCount = p.ItemCount,
            TotalQuantity = p.TotalQuantity
        }).ToList();

        var skipped = result.SkippedRecommendations.Select(s => new BulkOrderSkippedRecommendationDto
        {
            RecommendationId = s.RecommendationId,
            ItemName = s.ItemName,
            Reason = s.Reason
        }).ToList();

        var message = pos.Count > 0
            ? $"Created {pos.Count} purchase order(s) covering {result.TotalProcessed} critical recommendation(s). " +
              (skipped.Count > 0 ? $"{skipped.Count} skipped — see details." : "All resolved.")
            : (skipped.Count > 0
                ? $"No purchase orders created. All {skipped.Count} critical recommendations need a preferred supplier."
                : "No pending critical recommendations.");

        var dto = new BulkOrderResultDto
        {
            Success = pos.Count > 0,
            Message = message,
            TotalProcessed = result.TotalProcessed,
            PurchaseOrdersCreated = pos.Count,
            Skipped = skipped.Count,
            CreatedPurchaseOrders = pos,
            SkippedRecommendations = skipped
        };

        return Ok(ApiResponse<BulkOrderResultDto>.Ok(dto));
    }

    // ─── Manual AI forecast sync ────────────────────────────────────────────

    [HttpPost("run-forecast")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> RunForecast(CancellationToken ct)
    {
        var before = (await _forecasting.GetPendingRecommendationsAsync(branchId: null, ct)).Count;
        await _forecasting.RunDemandForecastingAsync(ct);
        var after = (await _forecasting.GetPendingRecommendationsAsync(branchId: null, ct)).Count;

        return Ok(ApiResponse<object>.Ok(
            new { before, after, generated = after - before },
            $"Forecast cycle complete. {after - before} new recommendation(s) generated."));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private Guid? CurrentUserId()
    {
        var raw = User.FindFirst("uid")?.Value;
        if (Guid.TryParse(raw, out var g) && g != Guid.Empty) return g;
        return null;
    }

    private static ApiResponse<RestockConversionResultDto> ToResult(bool ok, Guid id, string action, string message)
    {
        var result = new RestockConversionResultDto
        {
            Success = ok,
            Action = action,
            RecommendationId = id,
            Message = message
        };
        return ApiResponse<RestockConversionResultDto>.Ok(result, message);
    }

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

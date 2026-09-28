using System.ComponentModel.DataAnnotations;

namespace Store.Models.DTOs.Operations;

// ─── Outbound DTOs ────────────────────────────────────────────────────────────

/// <summary>
/// Restock recommendation projection returned by <c>GET /api/Restock/pending</c>.
/// Carries enough denormalised info for the UI to render the recommendation
/// without further round-trips (item name/barcode/cost, branch name, projected value).
/// </summary>
public class RestockRecommendationDto
{
    public Guid RecommendationId { get; set; }

    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;

    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemBarcode { get; set; }
    public string? ItemThumbnailUrl { get; set; }

    public int RecommendedQuantity { get; set; }

    /// <summary>Human-readable explanation produced by the velocity engine.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Days-of-stock remaining at current velocity (null when velocity is 0).</summary>
    public double? DaysOfStock { get; set; }

    /// <summary>Current branch-level on-hand quantity.</summary>
    public int CurrentStock { get; set; }

    /// <summary>Item-level reorder level (configurable per item).</summary>
    public int? ReorderLevel { get; set; }

    /// <summary>Estimated PO value at the item's preferred-supplier cost.</summary>
    public decimal ProjectedValue { get; set; }

    /// <summary>Severity bucket derived from the recommendation: "Critical" | "High" | "Medium".</summary>
    public string Severity { get; set; } = "Medium";

    public DateTime DateCreated { get; set; }
    public string Status { get; set; } = "Pending";
}

/// <summary>
/// Result of a conversion action (Transfer / PO / Dismiss). The UI uses
/// <see cref="Success"/> to decide whether to show an error banner, and
/// <see cref="Message"/> + <see cref="ReferenceNumber"/> for the success banner.
/// </summary>
public class RestockConversionResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Action { get; set; } // "transfer" | "purchase-order" | "dismiss"
    public int? ReferenceNumber { get; set; } // PO id or stock transfer id
    public Guid RecommendationId { get; set; }
}

/// <summary>
/// Lightweight projection of one restock row for KPI tile rendering.
/// Returned by <c>GET /api/Restock/summary</c>.
/// </summary>
public class RestockSummaryDto
{
    public int PendingCount { get; set; }
    public int CriticalCount { get; set; }
    public decimal ProjectedValue { get; set; }
    public string Currency { get; set; } = "XAF";
}

/// <summary>
/// One created purchase order inside a bulk-order response.
/// </summary>
public class BulkOrderPurchaseOrderDto
{
    public int PurchaseOrderId { get; set; }
    public Guid SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public int ItemCount { get; set; }
    public int TotalQuantity { get; set; }
}

/// <summary>
/// One recommendation that was skipped during bulk-order because no supplier
/// could be resolved. Surfaced in the UI so the operator can fix the catalog.
/// </summary>
public class BulkOrderSkippedRecommendationDto
{
    public Guid RecommendationId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Result of <c>POST /api/Restock/bulk-order-critical</c>.
/// </summary>
public class BulkOrderResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TotalProcessed { get; set; }
    public int PurchaseOrdersCreated { get; set; }
    public int Skipped { get; set; }
    public IReadOnlyList<BulkOrderPurchaseOrderDto> CreatedPurchaseOrders { get; set; } = Array.Empty<BulkOrderPurchaseOrderDto>();
    public IReadOnlyList<BulkOrderSkippedRecommendationDto> SkippedRecommendations { get; set; } = Array.Empty<BulkOrderSkippedRecommendationDto>();
}

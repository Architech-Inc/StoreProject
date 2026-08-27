using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Transfers;

public class StockTransferDto
{
    public int StockTransferId { get; set; }
    public int FromBranchId { get; set; }
    public string FromBranchName { get; set; } = string.Empty;
    public int ToBranchId { get; set; }
    public string ToBranchName { get; set; } = string.Empty;
    public string RequestedByUser { get; set; } = string.Empty;
    public string? ApprovedByUser { get; set; }
    public string? DispatchedByUser { get; set; }
    public string? ReceivedByUser { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public List<StockTransferItemDto> Items { get; set; } = new();

    // Computed & Valuation properties (XAF)
    public int TotalRequestedUnits => Items.Sum(i => i.RequestedQuantity);
    public int TotalDispatchedUnits => Items.Sum(i => i.DispatchedQuantity ?? 0);
    public int TotalReceivedUnits => Items.Sum(i => i.ReceivedQuantity ?? 0);
    public decimal TotalValuation => Items.Sum(i => i.LineValuation);
    public int DiscrepancyCount => Items.Count(i => i.DispatchedQuantity.HasValue && i.ReceivedQuantity.HasValue && i.ReceivedQuantity.Value < i.DispatchedQuantity.Value);
    public bool HasDiscrepancy => DiscrepancyCount > 0;
}

public class StockTransferItemDto
{
    public int StockTransferItemId { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemCode { get; set; }
    public string? CategoryName { get; set; }
    public decimal UnitCost { get; set; }
    public int RequestedQuantity { get; set; }
    public int? DispatchedQuantity { get; set; }
    public int? ReceivedQuantity { get; set; }
    public string? Notes { get; set; }

    public decimal LineValuation => (DispatchedQuantity ?? RequestedQuantity) * UnitCost;
    public int DiscrepancyQuantity => (DispatchedQuantity.HasValue && ReceivedQuantity.HasValue) 
        ? Math.Max(0, DispatchedQuantity.Value - ReceivedQuantity.Value) 
        : 0;
}

public class TransferMetricsDto
{
    public int TotalTransfers { get; set; }
    public int TotalRequested { get; set; }
    public int TotalApproved { get; set; }
    public int TotalInTransit { get; set; }
    public int TotalReceived { get; set; }
    public int TotalCancelled { get; set; }
    public int TotalTransferredUnits { get; set; }
    public decimal TotalInTransitValuationXaf { get; set; }
}

public class TransferFilterRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int? BranchId { get; set; }
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class CreateTransferRequest
{
    [Required]
    public int FromBranchId { get; set; }

    [Required]
    public int ToBranchId { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [Required, MinLength(1)]
    public List<TransferItemLine> Items { get; set; } = new();
}

public class TransferItemLine
{
    [Required]
    public Guid ItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int RequestedQuantity { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class ApproveTransferRequest
{
    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class RejectTransferRequest
{
    [Required, StringLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

public class DispatchTransferRequest
{
    [Required, MinLength(1)]
    public List<DispatchItemLine> Items { get; set; } = new();

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class DispatchItemLine
{
    public int StockTransferItemId { get; set; }

    [Range(0, int.MaxValue)]
    public int DispatchedQuantity { get; set; }
}

public class ReceiveTransferRequest
{
    [Required, MinLength(1)]
    public List<ReceiveItemLine> Items { get; set; } = new();

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class ReceiveItemLine
{
    public int StockTransferItemId { get; set; }

    [Range(0, int.MaxValue)]
    public int ReceivedQuantity { get; set; }
}

using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Procurement;

public class PurchaseOrderDto
{
    public int PurchaseOrderId { get; set; }
    public string? ReferenceNumber { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? SupplierEmail { get; set; }
    public string? SupplierPhone { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    public string RequestedByUser { get; set; } = string.Empty;
    public string? ApprovedByUser { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime DateCreated { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = new();

    public int LineCount => Items.Count;
    public int TotalOrderedUnits => Items.Sum(i => i.OrderedQuantity);
    public int TotalReceivedUnits => Items.Sum(i => i.ReceivedQuantity);
    public decimal TotalValuation => Items.Sum(i => i.LineValuation);
}

public class PurchaseOrderItemDto
{
    public int PurchaseOrderItemId { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int OrderedQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public int ReceivedQuantity { get; set; }
    public decimal LineValuation => OrderedQuantity * UnitCost;
    public decimal FulfilledValuation => ReceivedQuantity * UnitCost;
    public string? Notes { get; set; }
}

public class PurchaseOrderMetricsDto
{
    public int TotalOrders { get; set; }
    public int PendingApprovalCount { get; set; }
    public int AwaitingDeliveryCount { get; set; }
    public int FulfilledCount { get; set; }
    public decimal TotalCommittedValuationXaf { get; set; }
    public decimal TotalReceivedValuationXaf { get; set; }
}

public class PurchaseOrderFilterRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Status { get; set; }
    public Guid? SupplierId { get; set; }
    public int? BranchId { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class CreatePurchaseOrderRequest
{
    [Required]
    public Guid SupplierId { get; set; }

    public int? BranchId { get; set; }

    [StringLength(100)]
    public string? ReferenceNumber { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    [Required, MinLength(1, ErrorMessage = "At least one item is required.")]
    public List<CreatePurchaseOrderItemRequest> Items { get; set; } = new();
}

public class CreatePurchaseOrderItemRequest
{
    [Required]
    public Guid ItemId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Ordered quantity must be at least 1.")]
    public int OrderedQuantity { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Unit cost cannot be negative.")]
    public decimal UnitCost { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class ReceivePurchaseOrderRequest
{
    [Required, MinLength(1)]
    public List<ReceiveItemLine> Lines { get; set; } = new();
}

public class ReceiveItemLine
{
    public int PurchaseOrderItemId { get; set; }

    [Range(0, int.MaxValue)]
    public int ReceivedQuantity { get; set; }
}

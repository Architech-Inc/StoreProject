using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Procurement;

public class PurchaseOrderDto
{
    public int PurchaseOrderId { get; set; }
    public string? ReferenceNumber { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
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
}

public class PurchaseOrderItemDto
{
    public int PurchaseOrderItemId { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public int OrderedQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public int ReceivedQuantity { get; set; }
    public string? Notes { get; set; }
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

    [Range(1, int.MaxValue)]
    public int OrderedQuantity { get; set; }

    [Range(0, double.MaxValue)]
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

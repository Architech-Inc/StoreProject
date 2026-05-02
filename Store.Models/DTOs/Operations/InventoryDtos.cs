using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Operations;

public class GoodsReceiptLineRequest
{
    [Required]
    public Guid ItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? UnitCost { get; set; }
}

public class GoodsReceiptRequest
{
    public Guid? SupplierId { get; set; }
    public Guid? ItemsOrderId { get; set; }
    public string? ReferenceCode { get; set; }
    public string? Notes { get; set; }
    public List<GoodsReceiptLineRequest> Lines { get; set; } = new();
}

public class StockReturnRequest
{
    [Required]
    public Guid ItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public string Reason { get; set; } = "Customer return";
    public Guid? InvoiceId { get; set; }
}

public class StockAdjustmentAuditRequest
{
    [Required]
    public Guid ItemId { get; set; }

    public int QuantityDelta { get; set; }

    [Required, StringLength(200)]
    public string Reason { get; set; } = string.Empty;
}

public class InventoryOperationResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? ItemId { get; set; }
    public int? UpdatedStock { get; set; }
}

public class StockMovementDto
{
    public long StockMovementId { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public StockMovementType MovementType { get; set; }
    public int QuantityDelta { get; set; }
    public int StockBefore { get; set; }
    public int StockAfter { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceCode { get; set; }
    public DateTime DateCreated { get; set; }
}

public class ReorderSuggestionDto
{
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderLevel { get; set; }
    public int SuggestedOrderQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal EstimatedCost { get; set; }
}

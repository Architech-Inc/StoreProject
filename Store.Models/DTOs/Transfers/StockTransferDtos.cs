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
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public List<StockTransferItemDto> Items { get; set; } = new();
}

public class StockTransferItemDto
{
    public int StockTransferItemId { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int? DispatchedQuantity { get; set; }
    public int? ReceivedQuantity { get; set; }
    public string? Notes { get; set; }
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

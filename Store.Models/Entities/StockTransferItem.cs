using System.ComponentModel.DataAnnotations;
using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class StockTransferItem : BaseEntity
{
    [Key]
    public int StockTransferItemId { get; set; }

    public int StockTransferId { get; set; }
    public Guid ItemId { get; set; }

    public int RequestedQuantity { get; set; }

    /// <summary>Quantity actually dispatched (may differ from requested).</summary>
    public int? DispatchedQuantity { get; set; }

    /// <summary>Quantity confirmed received (may differ from dispatched).</summary>
    public int? ReceivedQuantity { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation
    public StockTransfer Transfer { get; set; } = null!;
    public Item Item { get; set; } = null!;
}

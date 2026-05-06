using System.ComponentModel.DataAnnotations;
using Store.Models.Entities.Base;

namespace Store.Models.Entities;

/// <summary>A line-item on a purchase order.</summary>
public class PurchaseOrderItem : BaseEntity
{
    [Key]
    public int PurchaseOrderItemId { get; set; }

    public int PurchaseOrderId { get; set; }

    public Guid ItemId { get; set; }

    public int OrderedQuantity { get; set; }

    public decimal UnitCost { get; set; }

    /// <summary>Cumulative quantity received so far (updated on partial/full receipt).</summary>
    public int ReceivedQuantity { get; set; } = 0;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Item Item { get; set; } = null!;
}

using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class StockMovement : BaseEntity
{
    public long StockMovementId { get; set; }
    public Guid ItemId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? ItemsOrderId { get; set; }
    public Guid? PerformedByUserId { get; set; }
    public StockMovementType MovementType { get; set; }
    public int QuantityDelta { get; set; }
    public int StockBefore { get; set; }
    public int StockAfter { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? UnitPrice { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceCode { get; set; }

    public Item Item { get; set; } = null!;
    public Invoice? Invoice { get; set; }
    public ItemsOrder? ItemsOrder { get; set; }
    public User? PerformedByUser { get; set; }
}

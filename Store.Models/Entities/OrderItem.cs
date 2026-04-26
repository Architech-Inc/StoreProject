using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class OrderItem : BaseEntity
{
    public int OrderItemId { get; set; }
    public Guid ItemsOrderId { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal? UnitCost { get; set; }
    public int QuantityOrdered { get; set; }
    public int QuantityReceived { get; set; }
    public decimal LineTotal { get; set; }

    // Navigation
    public ItemsOrder ItemsOrder { get; set; } = null!;
    public Item Item { get; set; } = null!;
}

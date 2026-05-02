using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class ItemsOrder : BaseEntity
{
    public Guid ItemsOrderId { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public decimal TotalAmount { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Supplier? Supplier { get; set; }
    public User? CreatedByUser { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}

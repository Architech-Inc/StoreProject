using Store.Models.Enums;

namespace Store.Models.DTOs.Orders;

public class OrderDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime DateCreated { get; set; }
    public List<OrderLineDto> Lines { get; set; } = new();
}

public class OrderLineDto
{
    public int OrderItemId { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int QuantityOrdered { get; set; }
    public int QuantityReceived { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal LineTotal { get; set; }
}

public class CreateOrderRequest
{
    public Guid? SupplierId { get; set; }
    public string? Notes { get; set; }
    public List<CreateOrderLineRequest> Lines { get; set; } = new();
}

public class CreateOrderLineRequest
{
    public Guid ItemId { get; set; }
    public int QuantityOrdered { get; set; }
}

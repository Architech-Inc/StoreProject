using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class Batch : BaseEntity
{
    public Guid BatchId { get; set; }
    public Guid ItemId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public DateTime ReceivedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }

    public Item Item { get; set; } = null!;
}

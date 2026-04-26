using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class Sale : BaseEntity
{
    public Guid SaleId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid ItemId { get; set; }
    public Guid? UserId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? UnitAbbreviation { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }

    // Navigation
    public Invoice Invoice { get; set; } = null!;
    public Item Item { get; set; } = null!;
    public User? User { get; set; }
}

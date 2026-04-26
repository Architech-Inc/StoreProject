using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class Discount : BaseEntity
{
    public int DiscountId { get; set; }
    public Guid? ItemId { get; set; }
    public Guid? ManagedByUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;

    public Item? Item { get; set; }
    public User? ManagedByUser { get; set; }
}

using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class ItemCategory : BaseEntity
{
    public int ItemCategoryId { get; set; }
    public Guid ItemId { get; set; }
    public int CategoryId { get; set; }

    public Item Item { get; set; } = null!;
    public Category Category { get; set; } = null!;
}

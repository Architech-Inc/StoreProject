using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class Category : BaseEntity
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }

    public ICollection<Item> Items { get; set; } = new List<Item>();
    public ICollection<ItemCategory> ItemCategories { get; set; } = new List<ItemCategory>();
}

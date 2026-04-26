using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class Unit : BaseEntity
{
    public int UnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Abbreviation { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Item> Items { get; set; } = new List<Item>();
}

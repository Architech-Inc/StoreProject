using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class ItemCode : BaseEntity
{
    public int ItemCodeId { get; set; }
    public Guid ItemId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string CodeType { get; set; } = string.Empty;

    public Item Item { get; set; } = null!;
}

using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class ItemExpiry : BaseEntity
{
    public int ItemExpiryId { get; set; }
    public Guid ItemId { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int? DaysWarningBefore { get; set; }

    public Item Item { get; set; } = null!;
}

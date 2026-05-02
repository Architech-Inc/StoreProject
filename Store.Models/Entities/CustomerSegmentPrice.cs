using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class CustomerSegmentPrice : BaseEntity
{
    public long CustomerSegmentPriceId { get; set; }
    public Guid ItemId { get; set; }
    public CustomerSegment Segment { get; set; } = CustomerSegment.Standard;
    public decimal PriceOverride { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;

    public Item Item { get; set; } = null!;
}

using Store.Models.Entities.Base;

namespace Store.Models.Entities;

public class BundleRule : BaseEntity
{
    public int BundleRuleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid TriggerItemId { get; set; }
    public Guid RewardItemId { get; set; }
    public int TriggerQuantity { get; set; } = 1;
    public int RewardQuantity { get; set; } = 1;
    public decimal RewardDiscountPercent { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;

    public Item TriggerItem { get; set; } = null!;
    public Item RewardItem { get; set; } = null!;
}

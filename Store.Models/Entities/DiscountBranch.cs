using Store.Models.Entities.Base;

namespace Store.Models.Entities;

/// <summary>
/// Scopes a discount/coupon promotion to specific physical branches.
/// </summary>
public class DiscountBranch : BaseEntity
{
    public int DiscountId { get; set; }
    public int BranchId { get; set; }

    // Navigation
    public Discount Discount { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}

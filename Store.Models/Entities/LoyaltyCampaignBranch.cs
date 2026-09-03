using Store.Models.Entities.Base;

namespace Store.Models.Entities;

/// <summary>
/// Scopes a loyalty point campaign to specific physical branches.
/// </summary>
public class LoyaltyCampaignBranch : BaseEntity
{
    public int LoyaltyCampaignId { get; set; }
    public int BranchId { get; set; }

    // Navigation
    public LoyaltyCampaign LoyaltyCampaign { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}

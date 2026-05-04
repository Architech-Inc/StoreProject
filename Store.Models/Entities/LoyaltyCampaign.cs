using System.ComponentModel.DataAnnotations;
using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

/// <summary>
/// Defines a loyalty point campaign (EX-FR-4.3).
/// A campaign can award bonus/multiplied points to all customers or a specific segment.
/// </summary>
public class LoyaltyCampaign : BaseEntity
{
    [Key]
    public int LoyaltyCampaignId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public LoyaltyCampaignType CampaignType { get; set; } = LoyaltyCampaignType.PointMultiplier;

    /// <summary>Null = all segments.</summary>
    public CustomerSegment? TargetSegment { get; set; }

    /// <summary>Used when CampaignType = PointMultiplier (e.g. 2.0 = double points).</summary>
    public decimal MultiplierFactor { get; set; } = 1m;

    /// <summary>Used when CampaignType = FixedBonusPoints.</summary>
    public int BonusPoints { get; set; } = 0;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}

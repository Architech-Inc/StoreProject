using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Loyalty;

public class LoyaltyCampaignDto
{
    public int LoyaltyCampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CampaignType { get; set; } = string.Empty;
    public string? TargetSegment { get; set; }
    public decimal MultiplierFactor { get; set; }
    public int BonusPoints { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsRunning { get; set; }
}

public class CreateCampaignRequest
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public LoyaltyCampaignType CampaignType { get; set; } = LoyaltyCampaignType.PointMultiplier;

    public CustomerSegment? TargetSegment { get; set; }

    [Range(0.01, 100)]
    public decimal MultiplierFactor { get; set; } = 1m;

    [Range(0, 100000)]
    public int BonusPoints { get; set; } = 0;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UpdateCampaignRequest
{
    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public LoyaltyCampaignType? CampaignType { get; set; }
    public CustomerSegment? TargetSegment { get; set; }

    [Range(0.01, 100)]
    public decimal? MultiplierFactor { get; set; }

    [Range(0, 100000)]
    public int? BonusPoints { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
}

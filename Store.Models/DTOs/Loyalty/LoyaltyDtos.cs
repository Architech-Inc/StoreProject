using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Loyalty;

public class LoyaltyAccountDto
{
    public int LoyaltyAccountId { get; set; }
    public Guid CustomerId { get; set; }
    public int Points { get; set; }
    public string Tier { get; set; } = "Bronze";
    public int LifetimePointsEarned { get; set; }
    public int TotalPointsRedeemed { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
}

public class LoyaltyMemberDto
{
    public int LoyaltyAccountId { get; set; }
    public Guid CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? FullImageUrl { get; set; }
    public string Segment { get; set; } = "Regular";
    public int Points { get; set; }
    public string Tier { get; set; } = "Bronze";
    public int LifetimePointsEarned { get; set; }
    public int TotalPointsRedeemed { get; set; }
    public decimal EstimatedMonetaryValue { get; set; }
    public int TierProgressPercentage { get; set; }
    public int NextTierThreshold { get; set; }
    public int PointsNeededForNextTier { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public DateTime DateEnrolled { get; set; }
}

public class LoyaltyMetricsDto
{
    public int TotalMembers { get; set; }
    public int ActiveMembers { get; set; }
    public int TotalPointsLiability { get; set; }
    public decimal PointsLiabilityValueXaf { get; set; }
    public int PointsEarnedThisMonth { get; set; }
    public int PointsRedeemedThisMonth { get; set; }
    public int BronzeCount { get; set; }
    public int SilverCount { get; set; }
    public int GoldCount { get; set; }
    public double VipTierRatio { get; set; }
}

public class LoyaltyMemberProfileDto
{
    public LoyaltyMemberDto Member { get; set; } = new();
    public List<LoyaltyTransactionDto> RecentTransactions { get; set; } = new();
    public List<LoyaltyCampaignDto> ActiveCampaigns { get; set; } = new();
    public decimal LifetimeSpend { get; set; }
    public int TotalOrders { get; set; }
}

public class GlobalLoyaltyTransactionDto
{
    public long LoyaltyTransactionId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public int Points { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public Guid? InvoiceId { get; set; }
    public string? Note { get; set; }
    public DateTime DateCreated { get; set; }
}

public class LoyaltyTransactionDto
{
    public long LoyaltyTransactionId { get; set; }
    public int Points { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public Guid? InvoiceId { get; set; }
    public string? Note { get; set; }
    public DateTime DateCreated { get; set; }
}

public class EarnPointsRequest
{
    public Guid CustomerId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Points must be at least 1.")]
    public int Points { get; set; }
    public Guid? InvoiceId { get; set; }
    public string? Note { get; set; }
}

public class RedeemPointsRequest
{
    public Guid CustomerId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Points must be at least 1.")]
    public int Points { get; set; }
    public string? Note { get; set; }
}

public class AdjustPointsRequest
{
    public Guid CustomerId { get; set; }
    public int Points { get; set; }
    public string? Note { get; set; }
}

public class ManagePointsRequest
{
    public Guid CustomerId { get; set; }
    public string ActionType { get; set; } = "Earn"; // Earn, Redeem, Adjust
    [Range(1, int.MaxValue, ErrorMessage = "Points must be at least 1.")]
    public int Points { get; set; }
    public Guid? InvoiceId { get; set; }
    public string? Note { get; set; }
}

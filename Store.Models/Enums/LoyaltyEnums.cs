namespace Store.Models.Enums;

public enum LoyaltyTier
{
    Bronze = 0,
    Silver = 1,
    Gold = 2
}

public enum LoyaltyTransactionType
{
    Earn = 0,
    Redeem = 1,
    Adjust = 2
}

public enum LoyaltyCampaignType
{
    /// <summary>Multiply earned points by MultiplierFactor.</summary>
    PointMultiplier = 0,
    /// <summary>Award a flat BonusPoints on top of normal earn.</summary>
    FixedBonusPoints = 1
}

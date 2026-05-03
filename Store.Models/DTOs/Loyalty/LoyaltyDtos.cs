using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Loyalty;

public class LoyaltyAccountDto
{
    public int LoyaltyAccountId { get; set; }
    public Guid CustomerId { get; set; }
    public int Points { get; set; }
    public string Tier { get; set; } = "Bronze";
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

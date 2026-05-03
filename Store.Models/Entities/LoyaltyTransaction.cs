using System.ComponentModel.DataAnnotations;
using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

/// <summary>
/// Records each loyalty point earn/redeem/adjust event (EX-FR-4.2).
/// </summary>
public class LoyaltyTransaction : BaseEntity
{
    [Key]
    public long LoyaltyTransactionId { get; set; }
    public int LoyaltyAccountId { get; set; }
    public Guid? InvoiceId { get; set; }
    public int Points { get; set; }
    public LoyaltyTransactionType TransactionType { get; set; }
    public string? Note { get; set; }

    // Navigation
    public CustomerLoyaltyAccount? LoyaltyAccount { get; set; }
    public Invoice? Invoice { get; set; }
}

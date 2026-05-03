using System.ComponentModel.DataAnnotations;
using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

/// <summary>
/// Tracks a customer's loyalty points balance and tier (EX-FR-4.1 / EX-FR-4.2).
/// </summary>
public class CustomerLoyaltyAccount : BaseEntity
{
    [Key]
    public int LoyaltyAccountId { get; set; }
    public Guid CustomerId { get; set; }
    public int Points { get; set; } = 0;
    public LoyaltyTier Tier { get; set; } = LoyaltyTier.Bronze;

    // Navigation
    public Customer? Customer { get; set; }
    public ICollection<LoyaltyTransaction> Transactions { get; set; } = new List<LoyaltyTransaction>();
}

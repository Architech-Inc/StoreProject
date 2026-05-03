using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class Invoice : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountTendered { get; set; }
    public decimal ChangeGiven { get; set; }
    public PaymentType PaymentType { get; set; } = PaymentType.Cash;
    public bool IsPaid { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public User? User { get; set; }
    public Customer? Customer { get; set; }

    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<InvoiceTender> Tenders { get; set; } = new List<InvoiceTender>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();
}

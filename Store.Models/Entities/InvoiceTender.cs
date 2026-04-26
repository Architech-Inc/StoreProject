using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class InvoiceTender : BaseEntity
{
    public int InvoiceTenderId { get; set; }
    public Guid InvoiceId { get; set; }
    public PaymentType PaymentType { get; set; }
    public decimal Amount { get; set; }
    public string? Reference { get; set; }

    public Invoice Invoice { get; set; } = null!;
}

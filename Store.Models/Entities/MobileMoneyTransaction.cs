using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

public class MobileMoneyTransaction : BaseEntity
{
    public Guid MobileMoneyTransactionId { get; set; }
    public Guid InvoiceId { get; set; }
    public MobileMoneyProvider Provider { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public MobileMoneyStatus Status { get; set; } = MobileMoneyStatus.Pending;

    /// <summary>Idempotency key — the provider's own transaction reference.</summary>
    public string? ProviderTransactionId { get; set; }

    /// <summary>Raw callback JSON payload received from the provider.</summary>
    public string? CallbackPayload { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public Invoice Invoice { get; set; } = null!;
}

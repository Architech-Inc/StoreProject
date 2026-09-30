using System.ComponentModel.DataAnnotations;
using Store.Models.Entities.Base;
using Store.Models.Enums;

namespace Store.Models.Entities;

/// <summary>
/// Discount override request submitted at POS when a cashier wants to apply
/// a discount beyond their authority (EX-FR-3.3 / FRA-1 / MCT-4).
/// Requires manager/supervisor approval before the discount is applied.
/// </summary>
public class DiscountOverrideRequest : BaseEntity
{
    [Key]
    public int DiscountOverrideRequestId { get; set; }

    /// <summary>The invoice or POS transaction this override relates to.</summary>
    public Guid? InvoiceId { get; set; }

    /// <summary>Item the override applies to (null = whole-invoice override).</summary>
    public Guid? ItemId { get; set; }

    /// <summary>Percentage or fixed amount being requested.</summary>
    public DiscountType OverrideType { get; set; } = DiscountType.Percentage;

    /// <summary>Value of the override (percentage 0-100 or fixed amount).</summary>
    public decimal OverrideValue { get; set; }

    [MaxLength(1000)]
    public string? Justification { get; set; }

    public DiscountOverrideStatus Status { get; set; } = DiscountOverrideStatus.Pending;

    /// <summary>Cashier/operator who requested the override.</summary>
    public Guid RequestedByUserId { get; set; }

    /// <summary>Manager who approved or rejected.</summary>
    public Guid? ReviewedByUserId { get; set; }

    [MaxLength(1000)]
    public string? ReviewNotes { get; set; }

    public DateTime? ReviewedAt { get; set; }

    // Wave 23.A — POS per-line override binding.

    /// <summary>
    /// Opaque client-side session id (32-char URL-safe base64) tying the
    /// override to a specific POS checkout attempt. Surfaced back to the
    /// client as <c>ClientSessionId</c> on <c>PosCheckoutRequest</c>; the
    /// checkout handler verifies the override was approved for this
    /// exact session (anti-replay on a different session).
    /// Null for the legacy invoice-scoped override flow.
    /// </summary>
    [MaxLength(64)]
    public string? PosSessionId { get; set; }

    /// <summary>
    /// SHA-256 hex of <c>(itemId, quantity)</c> pairs sorted, computed at
    /// approval time and re-verified at checkout. If the cart drifts between
    /// approval and checkout (cashier adds / removes items), the fingerprint
    /// mismatches and the override is rejected (server-side, HTTP 422).
    /// Null for the legacy invoice-scoped override flow.
    /// </summary>
    [MaxLength(64)]
    public string? CartFingerprint { get; set; }

    /// <summary>When the override transitioned to <c>Applied</c> via a successful checkout.</summary>
    public DateTime? AppliedAt { get; set; }

    // Navigation
    public User RequestedByUser { get; set; } = null!;
    public User? ReviewedByUser { get; set; }
    public Invoice? Invoice { get; set; }
    public Item? Item { get; set; }
}

using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Payments;

// --- Outbound (initiate) ---

public class InitiateMobileMoneyRequest
{
    [Required]
    public Guid InvoiceId { get; set; }

    [Required]
    public MobileMoneyProvider Provider { get; set; }

    [Required, Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, Range(1, double.MaxValue)]
    public decimal Amount { get; set; }
}

// --- Inbound callbacks ---

/// <summary>Simplified MTN MoMo Collections callback payload.</summary>
public class MtnMomoCallbackRequest
{
    public string? FinancialTransactionId { get; set; }
    public string? ExternalId { get; set; }
    public string? Amount { get; set; }
    public string? Currency { get; set; }
    public MtnMomoPayer? Payer { get; set; }
    public string? Status { get; set; }   // "SUCCESSFUL" | "FAILED"
    public MtnMomoReason? Reason { get; set; }
}

public class MtnMomoPayer
{
    public string? PartyIdType { get; set; }
    public string? PartyId { get; set; }
}

public class MtnMomoReason
{
    public string? Code { get; set; }
    public string? Message { get; set; }
}

/// <summary>Simplified Orange Money callback payload.</summary>
public class OrangeMoneyCallbackRequest
{
    public string? TransactionId { get; set; }
    public string? InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string? Phone { get; set; }
    public string? Status { get; set; }   // "SUCCESS" | "FAILURE"
    public string? Message { get; set; }
}

// --- Response DTOs ---

public class MobileMoneyTransactionDto
{
    public Guid MobileMoneyTransactionId { get; set; }
    public Guid InvoiceId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ProviderTransactionId { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime DateCreated { get; set; }
}

// --- Settlement ---

public class ChannelSettlementDto
{
    public string Channel { get; set; } = string.Empty;
    public PaymentType PaymentType { get; set; }
    public decimal TotalAmount { get; set; }
    public int InvoiceCount { get; set; }
}

public class SettlementReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalInvoices { get; set; }
    public decimal TotalSales { get; set; }
    public List<ChannelSettlementDto> ByChannel { get; set; } = new();
    public List<MobileMoneyTransactionDto> PendingMobileMoneyTransactions { get; set; } = new();
}

// ─── MT-02 — PayDunya / aggregator DTOs ─────────────────────────────────────

public class CreateInvoiceRequest
{
    /// <summary>Human-readable description of what's being billed.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Amount in the smallest currency unit (XAF: whole numbers).</summary>
    public int TotalAmount { get; set; }

    /// <summary>Currency code — XAF by default.</summary>
    public string Currency { get; set; } = "XAF";

    /// <summary>Internal tenant ID — echoed back on the IPN.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Internal plan tier identifier — echoed back on the IPN.</summary>
    public string PlanId { get; set; } = string.Empty;

    /// <summary>Optional payment-method filter (mtn-ci, orange-money-ci, wave, card).</summary>
    public string? Channel { get; set; }

    /// <summary>URL PayDunya redirects to after the user completes or cancels.</summary>
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>URL PayDunya posts the IPN to.</summary>
    public string CallbackUrl { get; set; } = string.Empty;
}

public class CreateInvoiceResponse
{
    /// <summary>PayDunya's invoice token. Required for confirm + IPN correlation.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Hosted-checkout URL the user should be redirected to.</summary>
    public string CheckoutUrl { get; set; } = string.Empty;

    /// <summary>Provider response code — "00" is success in PayDunya parlance.</summary>
    public string ResponseCode { get; set; } = string.Empty;

    /// <summary>Provider description, useful for surfacing failures to ops.</summary>
    public string? Description { get; set; }
}

public class ConfirmPaymentResponse
{
    public string Token { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // completed | pending | cancelled | failed
    public string ResponseCode { get; set; } = string.Empty;
    public int? Amount { get; set; }
    public string? Currency { get; set; }
    public string? Channel { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public Guid? TenantId { get; set; }
    public string? PlanId { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}

public class PayDunyaIpnPayload
{
    public string? Token { get; set; }
    public string? Status { get; set; }
    public string? ResponseCode { get; set; }
    public int? Amount { get; set; }
    public string? Currency { get; set; }
    public string? Channel { get; set; }
    public Guid? TenantId { get; set; }
    public string? PlanId { get; set; }
    public DateTime? CompletedAt { get; set; }
}

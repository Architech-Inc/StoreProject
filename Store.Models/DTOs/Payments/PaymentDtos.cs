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

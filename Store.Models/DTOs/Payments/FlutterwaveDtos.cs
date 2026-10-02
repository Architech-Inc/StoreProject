using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Store.Models.DTOs.Payments;

/// <summary>
/// Strongly-typed options for Flutterwave integration.
/// Bound from <c>Payments:Flutterwave</c> configuration.
/// </summary>
public class FlutterwaveOptions
{
    public const string SectionName = "Payments:Flutterwave";

    public string? ClientId { get; set; }
    public string? PublicKey { get; set; }
    public string? SecretKey { get; set; }
    public string? EncryptionKey { get; set; }
    public string? SecretHash { get; set; }
    public string BaseUrl { get; set; } = "https://api.flutterwave.com/v3/";
    public string TokenUrl { get; set; } = "https://idp.flutterwave.com/realms/flutterwave/protocol/openid-connect/token";
}

// ─── Payment Link Creation (Hosted Checkout) ────────────────────────────────

public class FlutterwavePaymentLinkRequest
{
    [Required]
    public string TxRef { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public string Currency { get; set; } = "USD";

    [Required]
    public string RedirectUrl { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }

    public string Title { get; set; } = "ClexAn Foods";
    public string Description { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Metadata payload echoed back in webhooks and verification endpoints.
    /// </summary>
    public Dictionary<string, string> Meta { get; set; } = new();
}

public class FlutterwavePaymentLinkResponse
{
    public bool Success { get; set; }
    public string? PaymentLink { get; set; }
    public string? TxRef { get; set; }
    public string? Message { get; set; }
    public string? ResponseCode { get; set; }
}

// ─── Transaction Verification ───────────────────────────────────────────────

public class FlutterwaveVerifyResponse
{
    public long Id { get; set; }
    public string TxRef { get; set; } = string.Empty;
    public string? FlwRef { get; set; }
    public decimal Amount { get; set; }
    public decimal ChargedAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "successful", "failed", etc.
    public string? PaymentType { get; set; } // "card", "mobilemoney", "account", etc.
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public Guid? TenantId { get; set; }
    public string? PlanId { get; set; }
    public Guid? InvoiceId { get; set; }
    public DateTime? CreatedAt { get; set; }
}

// ─── Webhook Payload ────────────────────────────────────────────────────────

public class FlutterwaveWebhookPayload
{
    [JsonPropertyName("event")]
    public string? Event { get; set; }

    [JsonPropertyName("data")]
    public FlutterwaveWebhookData? Data { get; set; }
}

public class FlutterwaveWebhookData
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("tx_ref")]
    public string? TxRef { get; set; }

    [JsonPropertyName("flw_ref")]
    public string? FlwRef { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("charged_amount")]
    public decimal ChargedAmount { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("payment_type")]
    public string? PaymentType { get; set; }

    [JsonPropertyName("customer")]
    public FlutterwaveCustomerData? Customer { get; set; }

    [JsonPropertyName("meta")]
    public Dictionary<string, object>? Meta { get; set; }
}

public class FlutterwaveCustomerData
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }
}

// ─── Store / In-Store POS Initiate Request ──────────────────────────────────

public class InitiateFlutterwaveStorePaymentRequest
{
    [Required]
    public Guid InvoiceId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public string Currency { get; set; } = "XAF";

    [Required, EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? RedirectUrl { get; set; }
}

public class InitiateFlutterwaveStorePaymentResponse
{
    public Guid TransactionId { get; set; }
    public string TxRef { get; set; } = string.Empty;
    public string PaymentLink { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
}

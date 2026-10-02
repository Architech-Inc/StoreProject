using Store.Models.DTOs.Payments;

namespace Store.Models.Interfaces.Services;

/// <summary>
/// Service contract for Flutterwave payment integration.
/// Supports both platform SaaS subscription billing and in-store POS customer payments.
/// </summary>
public interface IFlutterwavePaymentService
{
    /// <summary>
    /// Creates a Flutterwave hosted checkout payment link.
    /// </summary>
    Task<FlutterwavePaymentLinkResponse> CreatePaymentLinkAsync(FlutterwavePaymentLinkRequest request, CancellationToken ct = default);

    /// <summary>
    /// Verifies transaction details directly against Flutterwave's API.
    /// Standard zero-trust verification step for both webhooks and return URL callbacks.
    /// </summary>
    Task<FlutterwaveVerifyResponse?> VerifyTransactionAsync(long transactionId, CancellationToken ct = default);

    /// <summary>
    /// Validates the Flutterwave webhook signature hash (<c>verif-hash</c> header).
    /// Uses constant-time equality comparison to prevent timing attacks.
    /// </summary>
    bool VerifyWebhookHash(string? providedHash);
}

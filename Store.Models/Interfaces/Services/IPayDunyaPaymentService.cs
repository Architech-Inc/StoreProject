using Store.Models.DTOs.Payments;

namespace Store.Models.Interfaces.Services;

/// <summary>
/// MT-02 — PayDunya / aggregator payment provider.
///
/// PayDunya is a Senegal/WAEMU-originated payment aggregator that unifies
/// MTN MoMo, Orange Money, Wave, Visa/Mastercard, and several local payment
/// methods behind a single hosted checkout. Used here for the customer-facing
/// subscription flow (Tenant Portal billing).
///
/// The abstraction is intentionally provider-agnostic so a future Stripe or
/// CinetPay implementation can drop in without changing the controllers.
/// </summary>
public interface IPayDunyaPaymentService
{
    /// <summary>
    /// Create a hosted-checkout invoice. Returns a <see cref="CreateInvoiceResponse"/>
    /// carrying the PayDunya token + the URL the user should be redirected to.
    /// </summary>
    Task<CreateInvoiceResponse> CreateInvoiceAsync(CreateInvoiceRequest request, CancellationToken ct = default);

    /// <summary>
    /// Confirm the status of a previously created invoice. Used by both the
    /// webhook handler (to enrich the IPN payload) and the tenant portal's
    /// "is my payment done yet?" polling.
    /// </summary>
    Task<ConfirmPaymentResponse> ConfirmPaymentAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Verify the HMAC-SHA256 signature on a PayDunya IPN payload. Constant-
    /// time compare — see PayDunya docs.
    /// </summary>
    bool VerifyIpnSignature(string payloadJson, string signatureHex);
}
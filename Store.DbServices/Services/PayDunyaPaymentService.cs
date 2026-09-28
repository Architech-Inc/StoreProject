using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Store.Models.DTOs.Payments;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

/// <summary>
/// MT-02 — PayDunya aggregator client + IPN verifier.
///
/// Wraps PayDunya's hosted-checkout API in a typed HttpClient so DNS
/// rotation + connection pooling are owned by the framework. The endpoint
/// is selected by config (sandbox vs live) and bound to a master key +
/// private key + token.
///
/// API contract is documented at https://developers.paydunya.com.
/// Endpoint: POST /api/v1/checkout-invoice/create
/// Confirm:   GET  /api/v1/checkout-invoice/confirm/{token}
/// IPN:       POST to your callback URL with X-PayDunya-Signature header
///           (HMAC-SHA256 of the raw body, hex, lowercase).
///
/// Failures are logged at Warning, surfaced to the caller as null/empty
/// responses so the caller can decide whether to retry, queue, or fail
/// the user-facing flow gracefully.
/// </summary>
public class PayDunyaPaymentService : IPayDunyaPaymentService
{
    public const string DefaultSandboxBase = "https://app.paydunya.com/sandbox/api/v1";
    public const string DefaultLiveBase = "https://app.paydunya.com/api/v1";

    private readonly HttpClient _http;
    private readonly ILogger<PayDunyaPaymentService> _logger;
    private readonly string _masterKey;
    private readonly string _privateKey;
    private readonly string _publicKey;
    private readonly string _token;
    private readonly string _webhookSecret;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public PayDunyaPaymentService(
        HttpClient http,
        ILogger<PayDunyaPaymentService> logger,
        IOptions<PayDunyaOptions> options)
    {
        _http = http;
        _logger = logger;
        var opts = options.Value;
        _masterKey = opts.MasterKey ?? string.Empty;
        _privateKey = opts.PrivateKey ?? string.Empty;
        _publicKey = opts.PublicKey ?? string.Empty;
        _token = opts.Token ?? string.Empty;
        _webhookSecret = opts.WebhookSecret ?? string.Empty;

        if (string.IsNullOrEmpty(opts.BaseUrl))
        {
            _http.BaseAddress = new Uri(opts.UseSandbox ? DefaultSandboxBase : DefaultLiveBase);
        }
        else
        {
            _http.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
        }

        // Required by PayDunya ToS.
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("StoreProject/1.0 (PayDunya-Aggregator)");
    }

    public async Task<CreateInvoiceResponse> CreateInvoiceAsync(
        CreateInvoiceRequest request,
        CancellationToken ct = default)
    {
        if (!IsConfigured())
        {
            _logger.LogWarning("PayDunya client invoked without keys configured; returning empty invoice.");
            return new CreateInvoiceResponse { Token = string.Empty, CheckoutUrl = string.Empty, ResponseCode = "NOT_CONFIGURED" };
        }

        // PayDunya's expected body shape — wraps our flat DTO inside their
        // invoice / store / custom_data / actions hierarchy.
        var body = new
        {
            invoice = new
            {
                items = new[]
                {
                    new
                    {
                        name = request.Description,
                        quantity = 1,
                        unit_price = request.TotalAmount,
                        total_price = request.TotalAmount,
                        description = request.Description
                    }
                },
                total_amount = request.TotalAmount,
                description = request.Description
            },
            store = new
            {
                name = "ClexAn Foods POS",
                tagline = "Multi-tenant retail operations"
            },
            custom_data = new
            {
                tenant_id = request.TenantId.ToString("N"),
                plan_id = request.PlanId
            },
            actions = new
            {
                return_url = request.ReturnUrl,
                cancel_url = request.ReturnUrl,
                callback_url = request.CallbackUrl
            },
            channels = string.IsNullOrWhiteSpace(request.Channel)
                ? null
                : new[] { request.Channel }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "checkout-invoice/create")
        {
            Content = JsonContent.Create(body, options: JsonOpts)
        };
        AddAuthHeaders(req);

        try
        {
            using var resp = await _http.SendAsync(req, ct);
            var raw = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("PayDunya create-invoice failed: {Status} {Body}", resp.StatusCode, raw);
                return new CreateInvoiceResponse { ResponseCode = ((int)resp.StatusCode).ToString(), Description = raw };
            }

            using var doc = JsonDocument.Parse(raw);
            var responseCode = doc.RootElement.TryGetProperty("response_code", out var rc) ? rc.GetString() ?? "" : "";
            var token = doc.RootElement.TryGetProperty("token", out var tk) ? tk.GetString() ?? "" : "";
            var description = doc.RootElement.TryGetProperty("description", out var desc) ? desc.GetString() : null;
            var checkoutUrl = $"https://app.paydunya.com/sandbox-checkout/invoice/{token}/{_token}";
            if (!_http.BaseAddress!.AbsoluteUri.Contains("sandbox", StringComparison.OrdinalIgnoreCase))
            {
                checkoutUrl = $"https://app.paydunya.com/checkout/invoice/{token}/{_token}";
            }

            return new CreateInvoiceResponse
            {
                Token = token,
                CheckoutUrl = checkoutUrl,
                ResponseCode = responseCode,
                Description = description
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayDunya CreateInvoice threw.");
            return new CreateInvoiceResponse { ResponseCode = "EXCEPTION", Description = ex.Message };
        }
    }

    public async Task<ConfirmPaymentResponse> ConfirmPaymentAsync(string token, CancellationToken ct = default)
    {
        if (!IsConfigured() || string.IsNullOrWhiteSpace(token))
        {
            return new ConfirmPaymentResponse { Token = token, Status = "unknown", ResponseCode = "NOT_CONFIGURED" };
        }

        using var req = new HttpRequestMessage(HttpMethod.Get, $"checkout-invoice/confirm/{token}");
        AddAuthHeaders(req);

        try
        {
            using var resp = await _http.SendAsync(req, ct);
            var raw = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                return new ConfirmPaymentResponse { Token = token, Status = "failed", ResponseCode = ((int)resp.StatusCode).ToString() };
            }

            using var doc = JsonDocument.Parse(raw);
            var status = doc.RootElement.TryGetProperty("status", out var st) ? st.GetString() ?? "" : "";
            var rc = doc.RootElement.TryGetProperty("response_code", out var r) ? r.GetString() ?? "" : "";
            int? amount = null;
            if (doc.RootElement.TryGetProperty("total_amount", out var a) && a.ValueKind == JsonValueKind.Number && a.TryGetInt32(out var aValue))
            {
                amount = aValue;
            }
            string? currency = doc.RootElement.TryGetProperty("currency", out var cu) ? cu.GetString() : null;
            string? channel = doc.RootElement.TryGetProperty("channel", out var ch) ? ch.GetString() : null;

            Guid? tenantId = null;
            string? planId = null;
            if (doc.RootElement.TryGetProperty("custom_data", out var cd))
            {
                if (cd.TryGetProperty("tenant_id", out var tid) && Guid.TryParse(tid.GetString(), out var parsed))
                    tenantId = parsed;
                if (cd.TryGetProperty("plan_id", out var pid))
                    planId = pid.GetString();
            }

            return new ConfirmPaymentResponse
            {
                Token = token,
                Status = status,
                ResponseCode = rc,
                Amount = amount,
                Currency = currency,
                Channel = channel,
                TenantId = tenantId,
                PlanId = planId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayDunya ConfirmPayment threw for token {Token}", token);
            return new ConfirmPaymentResponse { Token = token, Status = "failed", ResponseCode = "EXCEPTION" };
        }
    }

    public bool VerifyIpnSignature(string payloadJson, string signatureHex)
    {
        if (string.IsNullOrEmpty(_webhookSecret) || string.IsNullOrEmpty(payloadJson) || string.IsNullOrEmpty(signatureHex))
        {
            return false;
        }

        try
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadJson));
            var expectedHex = Convert.ToHexString(computed).ToLowerInvariant();
            var providedHex = signatureHex.Trim().ToLowerInvariant();
            // Constant-time compare so we don't leak timing info.
            return CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(expectedHex),
                Encoding.ASCII.GetBytes(providedHex));
        }
        catch
        {
            return false;
        }
    }

    private void AddAuthHeaders(HttpRequestMessage req)
    {
        req.Headers.Add("PAYDUNYA-MASTER-KEY", _masterKey);
        req.Headers.Add("PAYDUNYA-PRIVATE-KEY", _privateKey);
        req.Headers.Add("PAYDUNYA-PUBLIC-KEY", _publicKey);
        req.Headers.Add("PAYDUNYA-TOKEN", _token);
    }

    private bool IsConfigured() =>
        !string.IsNullOrEmpty(_masterKey)
        && !string.IsNullOrEmpty(_privateKey)
        && !string.IsNullOrEmpty(_token);
}

/// <summary>
/// MT-02 — strongly-typed options for <see cref="PayDunyaPaymentService"/>.
/// Bound from <c>Payments:PayDunya</c> config section.
/// </summary>
public class PayDunyaOptions
{
    public const string SectionName = "Payments:PayDunya";

    public bool UseSandbox { get; set; } = true;
    public string? BaseUrl { get; set; }
    public string? MasterKey { get; set; }
    public string? PrivateKey { get; set; }
    public string? PublicKey { get; set; }
    public string? Token { get; set; }
    public string? WebhookSecret { get; set; }
}
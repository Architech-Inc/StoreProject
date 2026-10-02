using System.Net.Http.Headers;
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
/// Flutterwave payment gateway client and webhook verifier.
/// Supports both platform SaaS subscription billing and in-store customer invoicing/POS.
///
/// API documentation: https://developer.flutterwave.com/docs/
/// </summary>
public class FlutterwavePaymentService : IFlutterwavePaymentService
{
    public const string DefaultBaseUrl = "https://api.flutterwave.com/v3/";

    private readonly HttpClient _http;
    private readonly ILogger<FlutterwavePaymentService> _logger;
    private readonly string _secretKey;
    private readonly string _publicKey;
    private readonly string _secretHash;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public FlutterwavePaymentService(
        HttpClient http,
        ILogger<FlutterwavePaymentService> logger,
        IOptions<FlutterwaveOptions> options)
    {
        _http = http;
        _logger = logger;
        var opts = options.Value;

        _secretKey = opts.SecretKey?.Trim() ?? string.Empty;
        _publicKey = opts.PublicKey?.Trim() ?? string.Empty;
        _secretHash = opts.SecretHash?.Trim() ?? string.Empty;

        var baseUrl = string.IsNullOrWhiteSpace(opts.BaseUrl) ? DefaultBaseUrl : opts.BaseUrl.TrimEnd('/') + "/";
        _http.BaseAddress = new Uri(baseUrl);

        _http.DefaultRequestHeaders.UserAgent.ParseAdd("StoreProject/1.0 (Flutterwave-PaymentGateway)");
        if (!string.IsNullOrEmpty(_secretKey) && !_secretKey.StartsWith("OVERRIDE_ME", StringComparison.OrdinalIgnoreCase))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _secretKey);
        }
    }

    public async Task<FlutterwavePaymentLinkResponse> CreatePaymentLinkAsync(
        FlutterwavePaymentLinkRequest request,
        CancellationToken ct = default)
    {
        if (!IsConfigured())
        {
            _logger.LogWarning("Flutterwave client invoked without valid SecretKey configured; returning unconfigured response.");
            return new FlutterwavePaymentLinkResponse
            {
                Success = false,
                ResponseCode = "NOT_CONFIGURED",
                Message = "Flutterwave is not configured on this environment."
            };
        }

        var payload = new
        {
            tx_ref = request.TxRef,
            amount = request.Amount,
            currency = request.Currency,
            redirect_url = request.RedirectUrl,
            meta = request.Meta,
            customer = new
            {
                email = request.CustomerEmail,
                name = request.CustomerName ?? string.Empty,
                phonenumber = request.CustomerPhone ?? string.Empty
            },
            customizations = new
            {
                title = request.Title,
                description = request.Description,
                logo = request.LogoUrl
            }
        };

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "payments")
            {
                Content = JsonContent.Create(payload, options: JsonOpts)
            };
            AddAuthHeader(req);

            using var resp = await _http.SendAsync(req, ct);
            var raw = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Flutterwave CreatePaymentLink failed: {StatusCode} {Body}", resp.StatusCode, raw);
                return new FlutterwavePaymentLinkResponse
                {
                    Success = false,
                    ResponseCode = ((int)resp.StatusCode).ToString(),
                    Message = raw
                };
            }

            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            var status = root.TryGetProperty("status", out var st) ? st.GetString() : null;
            var message = root.TryGetProperty("message", out var msg) ? msg.GetString() : null;

            string? link = null;
            if (root.TryGetProperty("data", out var data) && data.TryGetProperty("link", out var linkProp))
            {
                link = linkProp.GetString();
            }

            var isSuccess = string.Equals(status, "success", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(link);

            return new FlutterwavePaymentLinkResponse
            {
                Success = isSuccess,
                PaymentLink = link,
                TxRef = request.TxRef,
                Message = message,
                ResponseCode = isSuccess ? "00" : status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Flutterwave CreatePaymentLink threw for TxRef {TxRef}", request.TxRef);
            return new FlutterwavePaymentLinkResponse
            {
                Success = false,
                ResponseCode = "EXCEPTION",
                Message = ex.Message
            };
        }
    }

    public async Task<FlutterwaveVerifyResponse?> VerifyTransactionAsync(long transactionId, CancellationToken ct = default)
    {
        if (!IsConfigured() || transactionId <= 0)
        {
            _logger.LogWarning("Flutterwave VerifyTransaction called without valid configuration or transaction ID {Id}", transactionId);
            return null;
        }

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, $"transactions/{transactionId}/verify");
            AddAuthHeader(req);

            using var resp = await _http.SendAsync(req, ct);
            var raw = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Flutterwave VerifyTransaction failed: {StatusCode} {Body}", resp.StatusCode, raw);
                return null;
            }

            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            if (!root.TryGetProperty("data", out var data))
            {
                return null;
            }

            var result = new FlutterwaveVerifyResponse
            {
                Id = data.TryGetProperty("id", out var idProp) ? idProp.GetInt64() : transactionId,
                TxRef = data.TryGetProperty("tx_ref", out var txProp) ? txProp.GetString() ?? string.Empty : string.Empty,
                FlwRef = data.TryGetProperty("flw_ref", out var flwProp) ? flwProp.GetString() : null,
                Amount = data.TryGetProperty("amount", out var amtProp) && amtProp.ValueKind == JsonValueKind.Number ? amtProp.GetDecimal() : 0m,
                ChargedAmount = data.TryGetProperty("charged_amount", out var chgProp) && chgProp.ValueKind == JsonValueKind.Number ? chgProp.GetDecimal() : 0m,
                Currency = data.TryGetProperty("currency", out var curProp) ? curProp.GetString() ?? string.Empty : string.Empty,
                Status = data.TryGetProperty("status", out var stProp) ? stProp.GetString() ?? string.Empty : string.Empty,
                PaymentType = data.TryGetProperty("payment_type", out var ptProp) ? ptProp.GetString() : null
            };

            if (data.TryGetProperty("created_at", out var createdProp) && createdProp.TryGetDateTime(out var dt))
            {
                result.CreatedAt = dt;
            }

            if (data.TryGetProperty("customer", out var cust))
            {
                result.CustomerEmail = cust.TryGetProperty("email", out var ce) ? ce.GetString() : null;
                result.CustomerName = cust.TryGetProperty("name", out var cn) ? cn.GetString() : null;
                result.CustomerPhone = cust.TryGetProperty("phone_number", out var cp) ? cp.GetString() : null;
            }

            if (data.TryGetProperty("meta", out var meta))
            {
                if (meta.TryGetProperty("tenant_id", out var tidProp) && Guid.TryParse(tidProp.GetString(), out var tid))
                {
                    result.TenantId = tid;
                }
                if (meta.TryGetProperty("plan_id", out var pidProp))
                {
                    result.PlanId = pidProp.GetString();
                }
                if (meta.TryGetProperty("invoice_id", out var iidProp) && Guid.TryParse(iidProp.GetString(), out var iid))
                {
                    result.InvoiceId = iid;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Flutterwave VerifyTransaction threw for TransactionId {TransactionId}", transactionId);
            return null;
        }
    }

    public bool VerifyWebhookHash(string? providedHash)
    {
        if (string.IsNullOrWhiteSpace(_secretHash) || string.IsNullOrWhiteSpace(providedHash))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(_secretHash.Trim());
        var providedBytes = Encoding.UTF8.GetBytes(providedHash.Trim());

        if (expectedBytes.Length != providedBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    private void AddAuthHeader(HttpRequestMessage req)
    {
        if (!string.IsNullOrEmpty(_secretKey) && !req.Headers.Contains("Authorization"))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _secretKey);
        }
    }

    private bool IsConfigured() =>
        !string.IsNullOrEmpty(_secretKey) &&
        !_secretKey.StartsWith("OVERRIDE_ME", StringComparison.OrdinalIgnoreCase);
}

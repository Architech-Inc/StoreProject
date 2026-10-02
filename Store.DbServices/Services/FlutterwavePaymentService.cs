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
    public const string DefaultTokenUrl = "https://idp.flutterwave.com/realms/flutterwave/protocol/openid-connect/token";

    private readonly HttpClient _http;
    private readonly ILogger<FlutterwavePaymentService> _logger;
    private readonly string? _clientId;
    private readonly string _secretKey;
    private readonly string _publicKey;
    private readonly string _secretHash;
    private readonly string _tokenUrl;

    private string? _cachedAccessToken;
    private DateTimeOffset _tokenExpiry = DateTimeOffset.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

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

        _clientId = string.IsNullOrWhiteSpace(opts.ClientId) ? null : opts.ClientId.Trim();
        _secretKey = opts.SecretKey?.Trim() ?? string.Empty;
        _publicKey = opts.PublicKey?.Trim() ?? string.Empty;
        _secretHash = opts.SecretHash?.Trim() ?? string.Empty;
        _tokenUrl = string.IsNullOrWhiteSpace(opts.TokenUrl) ? DefaultTokenUrl : opts.TokenUrl.Trim();

        var baseUrl = string.IsNullOrWhiteSpace(opts.BaseUrl) ? DefaultBaseUrl : opts.BaseUrl.TrimEnd('/') + "/";
        _http.BaseAddress = new Uri(baseUrl);

        _http.DefaultRequestHeaders.UserAgent.ParseAdd("StoreProject/1.0 (Flutterwave-PaymentGateway)");
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
            await AddAuthHeaderAsync(req, ct);

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
            await AddAuthHeaderAsync(req, ct);

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

    private async Task<string?> GetBearerTokenAsync(CancellationToken ct)
    {
        // Legacy v3 mode (no ClientId or placeholder) uses SecretKey directly as Bearer token
        if (string.IsNullOrWhiteSpace(_clientId) || _clientId.StartsWith("OVERRIDE_ME", StringComparison.OrdinalIgnoreCase))
        {
            return _secretKey;
        }

        // Return cached token if valid (with 60-second cushion)
        if (!string.IsNullOrEmpty(_cachedAccessToken) && DateTimeOffset.UtcNow < _tokenExpiry.AddSeconds(-60))
        {
            return _cachedAccessToken;
        }

        await _tokenLock.WaitAsync(ct);
        try
        {
            if (!string.IsNullOrEmpty(_cachedAccessToken) && DateTimeOffset.UtcNow < _tokenExpiry.AddSeconds(-60))
            {
                return _cachedAccessToken;
            }

            using var tokenReq = new HttpRequestMessage(HttpMethod.Post, _tokenUrl)
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", _clientId),
                    new KeyValuePair<string, string>("client_secret", _secretKey),
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                })
            };

            using var resp = await _http.SendAsync(tokenReq, ct);
            var raw = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("Flutterwave OAuth token request failed: {StatusCode} {Body}", resp.StatusCode, raw);
                return null;
            }

            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("access_token", out var tokenProp))
            {
                _cachedAccessToken = tokenProp.GetString();
                var expiresIn = doc.RootElement.TryGetProperty("expires_in", out var expProp) ? expProp.GetInt32() : 600;
                _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn);
                _logger.LogInformation("Refreshed Flutterwave OAuth token (expires in {ExpiresIn}s)", expiresIn);
                return _cachedAccessToken;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Flutterwave OAuth token");
            return null;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task AddAuthHeaderAsync(HttpRequestMessage req, CancellationToken ct)
    {
        if (req.Headers.Contains("Authorization")) return;

        var token = await GetBearerTokenAsync(ct);
        if (!string.IsNullOrEmpty(token))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private bool IsConfigured() =>
        !string.IsNullOrEmpty(_secretKey) &&
        !_secretKey.StartsWith("OVERRIDE_ME", StringComparison.OrdinalIgnoreCase);
}

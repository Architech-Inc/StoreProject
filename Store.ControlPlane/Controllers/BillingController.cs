using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Store.ControlPlane.Repositories;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Payments;
using Store.Models.Interfaces.Services;

namespace Store.ControlPlane.Controllers;

/// <summary>
/// Wave 17 + Wave 18 — billing surface for tenant operators.
/// Lives in ControlPlane so the IPN handler can reach
/// <see cref="SubscriptionReconciler"/> without circular project deps.
///
/// Endpoints:
///   - <c>POST /api/billing/paydunya/invoice</c> — auth, creates the hosted-checkout invoice.
///   - <c>GET /api/billing/paydunya/confirm/{token}</c> — auth, status polling.
///   - <c>POST /api/billing/paydunya/ipn</c> — anonymous, HMAC verified, reconciles payment.
///   - <c>GET /api/billing/paydunya/payments/{slug}</c> — auth, invoice history for the portal.
/// </summary>
[ApiController]
[Route("api/billing/paydunya")]
public class BillingController : ControllerBase
{
    private readonly IPayDunyaPaymentService _paydunya;
    private readonly SubscriptionReconciler _reconciler;
    private readonly ITenantRepository _tenantRepo;
    private readonly ILogger<BillingController> _logger;

    public BillingController(
        IPayDunyaPaymentService paydunya,
        SubscriptionReconciler reconciler,
        ITenantRepository tenantRepo,
        ILogger<BillingController> logger)
    {
        _paydunya = paydunya;
        _reconciler = reconciler;
        _tenantRepo = tenantRepo;
        _logger = logger;
    }

    [HttpPost("invoice")]
    [Authorize]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request, CancellationToken ct)
    {
        if (request.TotalAmount <= 0)
        {
            return BadRequest(ApiErrorResponse.From(
                ErrorCode.InvalidRequest, "TotalAmount must be greater than zero.",
                traceId: HttpContext.TraceIdentifier));
        }
        if (string.IsNullOrWhiteSpace(request.PlanId))
        {
            return BadRequest(ApiErrorResponse.From(
                ErrorCode.InvalidRequest, "PlanId is required.",
                traceId: HttpContext.TraceIdentifier));
        }

        var resp = await _paydunya.CreateInvoiceAsync(request, ct);
        if (resp.ResponseCode == "NOT_CONFIGURED" || string.IsNullOrEmpty(resp.Token))
        {
            return StatusCode(503, ApiErrorResponse.From(
                ErrorCode.PaymentProviderUnavailable,
                "PayDunya is not configured on this environment. Please contact support.",
                traceId: HttpContext.TraceIdentifier));
        }
        return Ok(ApiResponse<CreateInvoiceResponse>.Ok(resp, "Invoice created."));
    }

    [HttpGet("confirm/{token}")]
    [Authorize]
    public async Task<IActionResult> Confirm(string token, CancellationToken ct)
    {
        var resp = await _paydunya.ConfirmPaymentAsync(token, ct);
        // Wave 18 — opportunistically reconcile on a successful confirm so
        // tenants who refresh the page don't have to wait for the IPN to
        // propagate. The IPN is still the source of truth.
        if (string.Equals(resp.Status, "completed", StringComparison.OrdinalIgnoreCase))
        {
            await _reconciler.ReconcileAsync(
                resp.Token, resp.Status, resp.ResponseCode,
                resp.Amount, resp.Currency, resp.Channel,
                resp.TenantId, resp.PlanId,
                completedAtUtc: DateTime.UtcNow,
                failureReason: null,
                ct: ct);
        }
        return Ok(ApiResponse<ConfirmPaymentResponse>.Ok(resp));
    }

    [HttpPost("ipn")]
    [AllowAnonymous]
    public async Task<IActionResult> Ipn(CancellationToken ct)
    {
        var signature = Request.Headers["X-PayDunya-Signature"].ToString();
        if (string.IsNullOrWhiteSpace(signature))
        {
            _logger.LogWarning("PayDunya IPN missing X-PayDunya-Signature header.");
            return Unauthorized();
        }

        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(ct);
        Request.Body.Position = 0;

        if (string.IsNullOrEmpty(rawBody))
        {
            return BadRequest(new { message = "Empty body." });
        }

        if (!_paydunya.VerifyIpnSignature(rawBody, signature))
        {
            _logger.LogWarning("PayDunya IPN signature mismatch (len={BodyLen}).", rawBody.Length);
            return Unauthorized();
        }

        PayDunyaIpnPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<PayDunyaIpnPayload>(
                rawBody,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PayDunya IPN payload unparseable.");
            return BadRequest(new { message = "Invalid JSON." });
        }

        if (payload is null)
        {
            return BadRequest(new { message = "Null payload." });
        }

        // Wave 18 — reconcile the tenant row from the IPN.
        await _reconciler.ReconcileAsync(
            payload.Token, payload.Status, payload.ResponseCode,
            payload.Amount, payload.Currency, payload.Channel,
            payload.TenantId, payload.PlanId,
            payload.CompletedAt,
            failureReason: payload.Status is "failed" or "cancelled" ? payload.ResponseCode : null,
            ct: ct);

        return Ok(new { received = true });
    }

    /// <summary>
    /// Wave 18 — invoice history for a tenant. Used by the portal Billing
    /// page to render the "recent payments" table.
    /// </summary>
    [HttpGet("payments/{slug}")]
    [Authorize]
    public async Task<IActionResult> GetPayments(string slug, CancellationToken ct)
    {
        var tenant = await _tenantRepo.GetBySlugAsync(slug, ct);
        if (tenant is null)
        {
            return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Tenant not found."));
        }

        return Ok(ApiResponse<TenantPaymentHistoryDto>.Ok(new TenantPaymentHistoryDto
        {
            TenantId = tenant.TenantId,
            Slug = tenant.Slug,
            PlanTier = tenant.PlanTier.ToString(),
            SubscriptionStatus = tenant.SubscriptionStatus.ToString(),
            SubscriptionStartUtc = tenant.SubscriptionStartUtc,
            SubscriptionEndUtc = tenant.SubscriptionEndUtc,
            NextBillingAtUtc = tenant.NextBillingAtUtc,
            GracePeriodUntilUtc = tenant.GracePeriodUntilUtc,
            Payments = tenant.Payments
                .OrderByDescending(p => p.CreatedAtUtc)
                .Select(p => new TenantPaymentDto
                {
                    PaymentId = p.PaymentId,
                    Provider = p.Provider,
                    ProviderToken = p.ProviderToken,
                    Channel = p.Channel,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Status = p.Status,
                    PlanId = p.PlanId,
                    CreatedAtUtc = p.CreatedAtUtc,
                    CompletedAtUtc = p.CompletedAtUtc,
                    FailureReason = p.FailureReason
                })
                .ToList()
        }));
    }
}

/// <summary>Wave 18 — invoice history surface for the Billing page.</summary>
public class TenantPaymentHistoryDto
{
    public Guid TenantId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string PlanTier { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public DateTime? SubscriptionStartUtc { get; set; }
    public DateTime? SubscriptionEndUtc { get; set; }
    public DateTime? NextBillingAtUtc { get; set; }
    public DateTime? GracePeriodUntilUtc { get; set; }
    public List<TenantPaymentDto> Payments { get; set; } = new();
}

public class TenantPaymentDto
{
    public Guid PaymentId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ProviderToken { get; set; } = string.Empty;
    public string? Channel { get; set; }
    public int Amount { get; set; }
    public string Currency { get; set; } = "XAF";
    public string Status { get; set; } = string.Empty;
    public string? PlanId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string? FailureReason { get; set; }
}
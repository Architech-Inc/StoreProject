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
/// Flutterwave billing controller for tenant subscription operations.
/// Extends platform billing beyond PayDunya to support cards, mobile money, and bank transfers
/// across pan-African and international currencies (NGN, KES, GHS, ZAR, USD, XAF, etc.).
/// </summary>
[ApiController]
[Route("api/billing/flutterwave")]
public class FlutterwaveBillingController : ControllerBase
{
    private readonly IFlutterwavePaymentService _flutterwave;
    private readonly SubscriptionReconciler _reconciler;
    private readonly ITenantRepository _tenantRepo;
    private readonly ILogger<FlutterwaveBillingController> _logger;

    public FlutterwaveBillingController(
        IFlutterwavePaymentService flutterwave,
        SubscriptionReconciler reconciler,
        ITenantRepository tenantRepo,
        ILogger<FlutterwaveBillingController> logger)
    {
        _flutterwave = flutterwave;
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

        var txRef = $"flw-sub-{request.TenantId:N}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        var meta = new Dictionary<string, string>
        {
            ["tenant_id"] = request.TenantId.ToString(),
            ["plan_id"] = request.PlanId
        };

        var linkReq = new FlutterwavePaymentLinkRequest
        {
            TxRef = txRef,
            Amount = request.TotalAmount,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "USD" : request.Currency.ToUpperInvariant(),
            RedirectUrl = request.ReturnUrl,
            CustomerEmail = $"{request.TenantId:N}@billing.storeproject.internal",
            Title = "ClexAn Foods Subscription",
            Description = request.Description,
            Meta = meta
        };

        var resp = await _flutterwave.CreatePaymentLinkAsync(linkReq, ct);

        if (!resp.Success || string.IsNullOrEmpty(resp.PaymentLink))
        {
            if (resp.ResponseCode == "NOT_CONFIGURED")
            {
                return StatusCode(503, ApiErrorResponse.From(
                    ErrorCode.PaymentProviderUnavailable,
                    "Flutterwave is not configured on this environment. Please contact support.",
                    traceId: HttpContext.TraceIdentifier));
            }

            return BadRequest(ApiErrorResponse.From(
                ErrorCode.InvalidRequest,
                resp.Message ?? "Failed to create Flutterwave checkout link.",
                traceId: HttpContext.TraceIdentifier));
        }

        var result = new CreateInvoiceResponse
        {
            Token = txRef,
            CheckoutUrl = resp.PaymentLink,
            ResponseCode = "00",
            Description = resp.Message
        };

        return Ok(ApiResponse<CreateInvoiceResponse>.Ok(result, "Flutterwave invoice created."));
    }

    [HttpGet("verify/{transactionId:long}")]
    [Authorize]
    public async Task<IActionResult> Verify(long transactionId, CancellationToken ct)
    {
        var verified = await _flutterwave.VerifyTransactionAsync(transactionId, ct);
        if (verified is null)
        {
            return NotFound(ApiErrorResponse.From(
                ErrorCode.NotFound,
                $"Transaction {transactionId} could not be verified.",
                traceId: HttpContext.TraceIdentifier));
        }

        if (string.Equals(verified.Status, "successful", StringComparison.OrdinalIgnoreCase))
        {
            await _reconciler.ReconcileAsync(
                providerToken: verified.TxRef,
                status: "completed",
                responseCode: "00",
                amount: (int)verified.Amount,
                currency: verified.Currency,
                channel: verified.PaymentType,
                tenantId: verified.TenantId,
                planId: verified.PlanId,
                completedAtUtc: verified.CreatedAt ?? DateTime.UtcNow,
                failureReason: null,
                provider: "flutterwave",
                ct: ct);
        }

        var response = new ConfirmPaymentResponse
        {
            Token = verified.TxRef,
            Status = string.Equals(verified.Status, "successful", StringComparison.OrdinalIgnoreCase) ? "completed" : verified.Status,
            ResponseCode = "00",
            Amount = (int)verified.Amount,
            Currency = verified.Currency,
            Channel = verified.PaymentType,
            CustomerEmail = verified.CustomerEmail,
            CustomerPhone = verified.CustomerPhone,
            TenantId = verified.TenantId,
            PlanId = verified.PlanId,
            CompletedAtUtc = verified.CreatedAt
        };

        return Ok(ApiResponse<ConfirmPaymentResponse>.Ok(response));
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        var providedHash = Request.Headers["verif-hash"].ToString();
        if (!_flutterwave.VerifyWebhookHash(providedHash))
        {
            _logger.LogWarning("Flutterwave webhook rejected: invalid or missing verif-hash header.");
            return Unauthorized();
        }

        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync(ct);
        Request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(body))
        {
            return BadRequest(new { message = "Empty body." });
        }

        FlutterwaveWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<FlutterwaveWebhookPayload>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize Flutterwave webhook payload.");
            return BadRequest(new { message = "Invalid JSON." });
        }

        if (payload?.Data is null)
        {
            return BadRequest(new { message = "Missing transaction data." });
        }

        // Zero-trust verification: always query Flutterwave API to confirm transaction state
        var verified = await _flutterwave.VerifyTransactionAsync(payload.Data.Id, ct);
        if (verified is null)
        {
            _logger.LogWarning("Flutterwave webhook: verification failed for transaction {Id}", payload.Data.Id);
            return StatusCode(502, new { message = "Verification failed." });
        }

        var isCompleted = string.Equals(verified.Status, "successful", StringComparison.OrdinalIgnoreCase);

        await _reconciler.ReconcileAsync(
            providerToken: verified.TxRef,
            status: isCompleted ? "completed" : "failed",
            responseCode: isCompleted ? "00" : verified.Status,
            amount: (int)verified.Amount,
            currency: verified.Currency,
            channel: verified.PaymentType,
            tenantId: verified.TenantId,
            planId: verified.PlanId,
            completedAtUtc: verified.CreatedAt ?? DateTime.UtcNow,
            failureReason: isCompleted ? null : $"Status: {verified.Status}",
            provider: "flutterwave",
            ct: ct);

        return Ok(new { status = "success", verified = true });
    }
}

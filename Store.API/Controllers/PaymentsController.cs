using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Store.API.Attributes;
using Store.Models.DTOs.Operations;
using Store.Models.DTOs.Payments;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Invoices;
using Store.Models.Entities;
using Store.Models.Interfaces;

namespace Store.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IMobileMoneyService _momo;
    private readonly IFlutterwavePaymentService _flutterwave;
    private readonly IInvoiceService _invoiceService;
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _config;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IMobileMoneyService momo,
        IFlutterwavePaymentService flutterwave,
        IInvoiceService invoiceService,
        IUnitOfWork uow,
        IConfiguration config,
        ILogger<PaymentsController> logger)
    {
        _momo = momo;
        _flutterwave = flutterwave;
        _invoiceService = invoiceService;
        _uow = uow;
        _config = config;
        _logger = logger;
    }

    // ─── Initiate (requires auth) ─────────────────────────────────────────────

    [HttpPost("momo/initiate")]
    [Authorize(Policy = PermissionKeys.CashWrite)]
    [Audit("Initiate MoMo Payment", Category = "Payments")]
    public async Task<IActionResult> Initiate([FromBody] InitiateMobileMoneyRequest request, CancellationToken ct)
    {
        var tx = await _momo.InitiateAsync(request, ct);
        return Ok(tx);
    }

    // ─── Callbacks (no JWT — validated by HMAC-SHA256 over raw body) ────────

    [HttpPost("momo/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> MtnMomoCallback(CancellationToken ct)
    {
        if (!await ValidateHmacSignatureAsync(ct)) return Unauthorized();
        var callback = await ReadAndDeserializeAsync<MtnMomoCallbackRequest>(ct);
        if (callback is null) return BadRequest(new { message = "Invalid payload" });

        var result = await _momo.HandleMtnMomoCallbackAsync(callback, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpPost("orange/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> OrangeMoneyCallback(CancellationToken ct)
    {
        if (!await ValidateHmacSignatureAsync(ct)) return Unauthorized();
        var callback = await ReadAndDeserializeAsync<OrangeMoneyCallbackRequest>(ct);
        if (callback is null) return BadRequest(new { message = "Invalid payload" });

        var result = await _momo.HandleOrangeMoneyCallbackAsync(callback, ct);
        if (result is null) return NotFound();
        return Ok(result);
    }

    // ─── Flutterwave (Store / In-Store POS & Customer Invoicing) ──────────────

    [HttpPost("flutterwave/initiate")]
    [Authorize(Policy = PermissionKeys.CashWrite)]
    [Audit("Initiate Flutterwave Payment", Category = "Payments")]
    public async Task<IActionResult> InitiateFlutterwave([FromBody] InitiateFlutterwaveStorePaymentRequest request, CancellationToken ct)
    {
        if (request.Amount <= 0)
        {
            return BadRequest(new { message = "Amount must be greater than zero." });
        }

        var invoice = await _uow.Repository<Invoice>().GetByIdAsync(request.InvoiceId, ct);
        if (invoice is null)
        {
            return NotFound(new { message = $"Invoice {request.InvoiceId} not found." });
        }

        if (invoice.IsPaid)
        {
            return BadRequest(new { message = "Invoice is already fully paid." });
        }

        var txRef = $"flw-inv-{request.InvoiceId:N}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        var tx = new MobileMoneyTransaction
        {
            MobileMoneyTransactionId = Guid.NewGuid(),
            InvoiceId = request.InvoiceId,
            Provider = MobileMoneyProvider.Flutterwave,
            PhoneNumber = request.CustomerPhone ?? string.Empty,
            Amount = request.Amount,
            Status = MobileMoneyStatus.Pending,
            ProviderTransactionId = txRef
        };

        await _uow.Repository<MobileMoneyTransaction>().AddAsync(tx, ct);
        await _uow.SaveChangesAsync(ct);

        var redirectUrl = !string.IsNullOrWhiteSpace(request.RedirectUrl)
            ? request.RedirectUrl
            : $"{Request.Scheme}://{Request.Host}/api/payments/flutterwave/verify/{txRef}";

        var linkReq = new FlutterwavePaymentLinkRequest
        {
            TxRef = txRef,
            Amount = request.Amount,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "XAF" : request.Currency.ToUpperInvariant(),
            RedirectUrl = redirectUrl,
            CustomerEmail = request.CustomerEmail,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            Title = request.Title ?? $"Invoice #{invoice.InvoiceId.ToString()[..8]}",
            Description = request.Description ?? $"Payment for invoice #{invoice.InvoiceId.ToString()[..8]}",
            Meta = new Dictionary<string, string>
            {
                ["invoice_id"] = request.InvoiceId.ToString(),
                ["transaction_id"] = tx.MobileMoneyTransactionId.ToString()
            }
        };

        var resp = await _flutterwave.CreatePaymentLinkAsync(linkReq, ct);
        if (!resp.Success || string.IsNullOrEmpty(resp.PaymentLink))
        {
            tx.Status = MobileMoneyStatus.Failed;
            tx.CallbackPayload = resp.Message;
            _uow.Repository<MobileMoneyTransaction>().Update(tx);
            await _uow.SaveChangesAsync(ct);
            return BadRequest(new { message = resp.Message ?? "Failed to initiate Flutterwave payment link." });
        }

        return Ok(new InitiateFlutterwaveStorePaymentResponse
        {
            TransactionId = tx.MobileMoneyTransactionId,
            TxRef = txRef,
            PaymentLink = resp.PaymentLink,
            Status = "Pending"
        });
    }

    [HttpPost("flutterwave/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> FlutterwaveWebhook(CancellationToken ct)
    {
        var providedHash = Request.Headers["verif-hash"].ToString();
        if (!_flutterwave.VerifyWebhookHash(providedHash))
        {
            _logger.LogWarning("Flutterwave store webhook rejected: invalid or missing verif-hash header.");
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
            _logger.LogWarning(ex, "Failed to deserialize Flutterwave store webhook payload.");
            return BadRequest(new { message = "Invalid JSON." });
        }

        if (payload?.Data is null)
        {
            return BadRequest(new { message = "Missing transaction data." });
        }

        // Zero-trust verification: query Flutterwave directly
        var verified = await _flutterwave.VerifyTransactionAsync(payload.Data.Id, ct);
        if (verified is null)
        {
            _logger.LogWarning("Flutterwave store webhook: verification failed for transaction {Id}", payload.Data.Id);
            return StatusCode(502, new { message = "Verification failed." });
        }

        var tx = await _uow.Repository<MobileMoneyTransaction>().Query()
            .FirstOrDefaultAsync(t => t.ProviderTransactionId == verified.TxRef, ct);

        if (tx is null && verified.InvoiceId.HasValue)
        {
            tx = await _uow.Repository<MobileMoneyTransaction>().Query()
                .Where(t => t.InvoiceId == verified.InvoiceId.Value && t.Provider == MobileMoneyProvider.Flutterwave && t.Status == MobileMoneyStatus.Pending)
                .OrderByDescending(t => t.DateCreated)
                .FirstOrDefaultAsync(ct);
        }

        if (tx is null)
        {
            _logger.LogWarning("Flutterwave store webhook: no pending transaction found for TxRef={TxRef}, InvoiceId={InvoiceId}", verified.TxRef, verified.InvoiceId);
            return NotFound(new { message = "Transaction not found." });
        }

        if (tx.Status != MobileMoneyStatus.Pending)
        {
            return Ok(new { status = "already_processed" });
        }

        var isSuccess = string.Equals(verified.Status, "successful", StringComparison.OrdinalIgnoreCase);
        tx.Status = isSuccess ? MobileMoneyStatus.Completed : MobileMoneyStatus.Failed;
        tx.CompletedAtUtc = DateTime.UtcNow;
        tx.CallbackPayload = body;
        tx.LastModified = DateTime.UtcNow;
        _uow.Repository<MobileMoneyTransaction>().Update(tx);
        await _uow.SaveChangesAsync(ct);

        if (isSuccess)
        {
            var paymentType = string.Equals(verified.PaymentType, "card", StringComparison.OrdinalIgnoreCase)
                ? PaymentType.Card
                : PaymentType.MobileMoney;

            try
            {
                await _invoiceService.AddTenderAsync(tx.InvoiceId, new AddTenderRequest
                {
                    PaymentType = paymentType,
                    Amount = verified.Amount,
                    Reference = $"FLW-{verified.Id}"
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply tender for invoice {InvoiceId} via Flutterwave transaction {Id}", tx.InvoiceId, verified.Id);
            }
        }

        return Ok(new { status = "success", verified = true });
    }

    [HttpGet("flutterwave/verify/{transactionId:long}")]
    [Authorize(Policy = PermissionKeys.PaymentsRead)]
    public async Task<IActionResult> VerifyFlutterwave(long transactionId, CancellationToken ct)
    {
        var verified = await _flutterwave.VerifyTransactionAsync(transactionId, ct);
        return verified is not null ? Ok(verified) : NotFound();
    }

    // ─── Settlement report ────────────────────────────────────────────────────

    [HttpGet("settlement")]
    [Authorize(Policy = PermissionKeys.PaymentsRead)]
    public async Task<IActionResult> GetSettlement(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken ct)
    {
        var from = fromDate?.Date.ToUniversalTime() ?? DateTime.UtcNow.Date;
        var to = toDate?.Date.ToUniversalTime() ?? DateTime.UtcNow.Date;

        if (to < from)
            return BadRequest(new { message = "toDate must be >= fromDate." });

        var report = await _momo.GetSettlementReportAsync(from, to, ct);
        return Ok(report);
    }

    // ─── Transaction list ─────────────────────────────────────────────────────

    [HttpGet("momo")]
    [Authorize(Policy = PermissionKeys.PaymentsRead)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] MobileMoneyStatus? status = null,
        CancellationToken ct = default)
    {
        var rows = await _momo.GetTransactionsAsync(page, pageSize, status, ct);
        return Ok(rows);
    }

    [HttpGet("momo/{id:guid}")]
    [Authorize(Policy = PermissionKeys.PaymentsRead)]
    public async Task<IActionResult> GetTransactionById(Guid id, CancellationToken ct = default)
    {
        var tx = await _momo.GetTransactionByIdAsync(id, ct);
        return tx is not null ? Ok(tx) : NotFound();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Validates the X-Callback-Signature header against HMAC-SHA256(rawBody, secret).
    /// Header format: <c>X-Callback-Signature: sha256=&lt;hex&gt;</c> (lowercase hex).
    /// Reject any other scheme (the older <c>X-Callback-Key</c> static scheme is removed).
    /// </summary>
    private async Task<bool> ValidateHmacSignatureAsync(CancellationToken ct)
    {
        var secret = _config["Payments:MoMoCallbackKey"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            _logger.LogError("Payments:MoMoCallbackKey is not configured. Refusing to process callback.");
            return false;
        }

        // Read the raw body once. EnableBuffering allows the model binder to re-read it.
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(ct);
        Request.Body.Position = 0;

        if (string.IsNullOrEmpty(rawBody))
        {
            _logger.LogWarning("Empty MoMo callback body — rejected.");
            return false;
        }

        var header = Request.Headers["X-Callback-Signature"].ToString();
        if (string.IsNullOrEmpty(header) || !header.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("MoMo callback missing or malformed X-Callback-Signature header.");
            return false;
        }

        var providedHex = header["sha256=".Length..].Trim();
        byte[] expected;
        try
        {
            expected = HMACSHA256.HashData(
                Encoding.UTF8.GetBytes(secret),
                Encoding.UTF8.GetBytes(rawBody));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute HMAC for MoMo callback");
            return false;
        }

        byte[] provided;
        try { provided = Convert.FromHexString(providedHex); }
        catch { return false; }

        if (provided.Length != expected.Length) return false;
        return CryptographicOperations.FixedTimeEquals(expected, provided);
    }

    private async Task<T?> ReadAndDeserializeAsync<T>(CancellationToken ct) where T : class
    {
        Request.EnableBuffering();
        Request.Body.Position = 0;
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var raw = await reader.ReadToEndAsync(ct);
        if (string.IsNullOrEmpty(raw)) return null;
        try
        {
            return JsonSerializer.Deserialize<T>(raw, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize MoMo callback payload");
            return null;
        }
    }
}

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

namespace Store.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IMobileMoneyService _momo;
    private readonly IConfiguration _config;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IMobileMoneyService momo, IConfiguration config, ILogger<PaymentsController> logger)
    {
        _momo = momo;
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

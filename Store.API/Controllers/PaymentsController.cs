using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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

    public PaymentsController(IMobileMoneyService momo, IConfiguration config)
    {
        _momo = momo;
        _config = config;
    }

    // ─── Initiate (requires auth) ─────────────────────────────────────────────

    [HttpPost("momo/initiate")]
    [Authorize]
    public async Task<IActionResult> Initiate([FromBody] InitiateMobileMoneyRequest request, CancellationToken ct)
    {
        var tx = await _momo.InitiateAsync(request, ct);
        return Ok(tx);
    }

    // ─── Callbacks (no JWT — validated by shared secret header) ──────────────

    [HttpPost("momo/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> MtnMomoCallback([FromBody] MtnMomoCallbackRequest callback, CancellationToken ct)
    {
        if (!ValidateCallbackKey())
            return Unauthorized();

        var result = await _momo.HandleMtnMomoCallbackAsync(callback, ct);
        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost("orange/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> OrangeMoneyCallback([FromBody] OrangeMoneyCallbackRequest callback, CancellationToken ct)
    {
        if (!ValidateCallbackKey())
            return Unauthorized();

        var result = await _momo.HandleOrangeMoneyCallbackAsync(callback, ct);
        if (result is null)
            return NotFound();

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

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private bool ValidateCallbackKey()
    {
        var expectedKey = _config["Payments:MoMoCallbackKey"];
        if (string.IsNullOrWhiteSpace(expectedKey))
            return false;

        var providedKey = Request.Headers["X-Callback-Key"].ToString();
        return !string.IsNullOrEmpty(providedKey) &&
               string.Equals(providedKey, expectedKey, StringComparison.Ordinal);
    }
}

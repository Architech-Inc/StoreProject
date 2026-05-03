using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/pricing")]
[Authorize]
public class PricingController : ControllerBase
{
    private readonly IStoreOperationsService _ops;

    public PricingController(IStoreOperationsService ops)
    {
        _ops = ops;
    }

    [HttpGet("tax-profiles")]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> GetTaxProfiles(CancellationToken ct)
    {
        return Ok(await _ops.GetTaxProfilesAsync(ct));
    }

    [HttpPost("tax-profiles")]
    [Authorize(Policy = PermissionKeys.PricingWrite)]
    public async Task<IActionResult> UpsertTaxProfile([FromBody] UpsertTaxProfileRequest request, CancellationToken ct)
    {
        return Ok(await _ops.UpsertTaxProfileAsync(request, ct));
    }

    [HttpGet("bundles")]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> GetBundleRules(CancellationToken ct)
    {
        return Ok(await _ops.GetBundleRulesAsync(ct));
    }

    [HttpPost("bundles")]
    [Authorize(Policy = PermissionKeys.PricingWrite)]
    public async Task<IActionResult> UpsertBundleRule([FromBody] UpsertBundleRuleRequest request, CancellationToken ct)
    {
        return Ok(await _ops.UpsertBundleRuleAsync(request, ct));
    }

    [HttpGet("segment-pricing")]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> GetSegmentPricing(CancellationToken ct)
    {
        return Ok(await _ops.GetSegmentPricingsAsync(ct));
    }

    [HttpPost("segment-pricing")]
    [Authorize(Policy = PermissionKeys.PricingWrite)]
    public async Task<IActionResult> UpsertSegmentPricing([FromBody] UpsertSegmentPricingRequest request, CancellationToken ct)
    {
        return Ok(await _ops.UpsertSegmentPricingAsync(request, ct));
    }

    [HttpPost("preview")]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> Preview([FromBody] PricingPreviewRequest request, CancellationToken ct)
    {
        var result = await _ops.GetPricingPreviewAsync(request, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("promotions/effectiveness")]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> GetPromotionEffectiveness(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken ct)
    {
        var from = fromDate?.Date.ToUniversalTime() ?? DateTime.UtcNow.Date.AddDays(-30);
        var to = toDate?.Date.ToUniversalTime() ?? DateTime.UtcNow.Date;

        if (to < from)
            return BadRequest(new { message = "toDate must be >= fromDate." });

        var result = await _ops.GetPromotionEffectivenessAsync(from, to, ct);
        return Ok(result);
    }
}

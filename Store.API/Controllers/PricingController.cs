using System.Text;
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

    [HttpGet("metrics")]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> GetMetrics(CancellationToken ct)
    {
        return Ok(await _ops.GetPricingOpsMetricsAsync(ct));
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

    [HttpGet("export/csv")]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> ExportCsv([FromQuery] string? type, CancellationToken ct)
    {
        var targetType = (type ?? "all").ToLowerInvariant();
        var sb = new StringBuilder();

        if (targetType == "taxes")
        {
            var taxes = await _ops.GetTaxProfilesAsync(ct);
            sb.AppendLine("Tax Profile ID,Name,Rate Percent,Application Type,Status");
            foreach (var t in taxes)
            {
                sb.AppendLine($"{t.TaxProfileId},\"{t.Name.Replace("\"", "\"\"")}\",{t.RatePercent},{t.ApplicationType},{(t.IsActive ? "Active" : "Inactive")}");
            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"tax_profiles_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
        }
        else if (targetType == "bundles")
        {
            var bundles = await _ops.GetBundleRulesAsync(ct);
            sb.AppendLine("Bundle Rule ID,Rule Name,Trigger Product,Trigger Qty,Reward Product,Reward Qty,Reward Discount %,Valid From,Valid To,Status");
            foreach (var b in bundles)
            {
                sb.AppendLine($"{b.BundleRuleId},\"{b.Name.Replace("\"", "\"\"")}\",\"{b.TriggerItemName.Replace("\"", "\"\"")}\",{b.TriggerQuantity},\"{b.RewardItemName.Replace("\"", "\"\"")}\",{b.RewardQuantity},{b.RewardDiscountPercent},\"{b.ValidFrom:yyyy-MM-dd}\",\"{b.ValidTo:yyyy-MM-dd}\",{(b.IsActive ? "Active" : "Inactive")}");
            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"bundle_rules_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
        }
        else
        {
            var segments = await _ops.GetSegmentPricingsAsync(ct);
            sb.AppendLine("Segment Price ID,Product Name,Base Catalog Price (XAF),Unit Cost (XAF),Customer Tier,Override Price (XAF),Profit Margin %,Valid From,Valid To,Status");
            foreach (var s in segments)
            {
                sb.AppendLine($"{s.CustomerSegmentPriceId},\"{s.ItemName.Replace("\"", "\"\"")}\",{s.BaseUnitPrice},{s.UnitCostPrice},{s.Segment},{s.PriceOverride},{s.MarginPercent}%,\"{s.ValidFrom:yyyy-MM-dd}\",\"{s.ValidTo:yyyy-MM-dd}\",{(s.IsActive ? "Active" : "Inactive")}");
            }
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"segment_pricings_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
        }
    }
}

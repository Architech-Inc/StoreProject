using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Contracts;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Discounts;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DiscountsController : ControllerBase
{
    private readonly IDiscountService _discountService;

    public DiscountsController(IDiscountService discountService)
        => _discountService = discountService;

    [HttpGet("metrics")]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> GetMetrics(CancellationToken ct)
    {
        var metrics = await _discountService.GetMetricsAsync(ct);
        return Ok(ApiResponse<DiscountMetricsDto>.Ok(metrics));
    }

    [HttpGet("paged")]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> GetPaged([FromQuery] DiscountFilterRequest request, CancellationToken ct)
    {
        var result = await _discountService.GetDiscountsPagedAsync(request, ct);
        return Ok(ApiResponse<PagedResult<DiscountDto>>.Ok(result));
    }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly, [FromQuery] string? couponCode)
    {
        var list = await _discountService.GetAllAsync(activeOnly, couponCode);
        return Ok(ApiResponse<List<DiscountDto>>.Ok(list));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _discountService.GetByIdAsync(id);
        if (dto is null)
            return NotFound(ApiErrorResponse.From("not_found", "Discount not found", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<DiscountDto>.Ok(dto));
    }

    [HttpPost("simulate")]
    public async Task<IActionResult> Simulate([FromBody] DiscountSimulationRequest request, CancellationToken ct)
    {
        var result = await _discountService.SimulateDiscountAsync(request, ct);
        return Ok(ApiResponse<DiscountSimulationResult>.Ok(result));
    }

    [HttpGet("validate-coupon")]
    public async Task<IActionResult> ValidateCoupon([FromQuery] string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest(ApiErrorResponse.From("bad_request", "Coupon code is required", traceId: HttpContext.TraceIdentifier));

        var dto = await _discountService.ValidateCouponAsync(code);
        if (dto is null)
            return NotFound(ApiErrorResponse.From("not_found", "Coupon is invalid, expired, or exhausted", traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<DiscountDto>.Ok(dto));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.PricingWrite)]
    public async Task<IActionResult> Create([FromBody] CreateDiscountRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var dto = await _discountService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = dto.DiscountId }, ApiResponse<DiscountDto>.Ok(dto));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.PricingWrite)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDiscountRequest request)
    {
        var dto = await _discountService.UpdateAsync(id, request);
        if (dto is null)
            return NotFound(ApiErrorResponse.From("not_found", "Discount not found", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<DiscountDto>.Ok(dto));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermissionKeys.PricingWrite)]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _discountService.DeleteAsync(id);
        if (!ok)
            return NotFound(ApiErrorResponse.From("not_found", "Discount not found", traceId: HttpContext.TraceIdentifier));
        return NoContent();
    }

    [HttpPost("{id:int}/increment-usage")]
    [Authorize(Policy = PermissionKeys.CashWrite)]
    public async Task<IActionResult> IncrementUsage(int id)
    {
        await _discountService.IncrementUsageAsync(id);
        return NoContent();
    }

    [HttpGet("export/csv")]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> ExportCsv([FromQuery] DiscountFilterRequest request, CancellationToken ct)
    {
        request.Page = 1;
        request.PageSize = 2000;
        var paged = await _discountService.GetDiscountsPagedAsync(request, ct);

        var sb = new StringBuilder();
        sb.AppendLine("ID,Name,Type,Value,Scope,Target Item/Category,Min Qty,Customer Segment,Coupon Code,Used Count,Max Uses,Valid From,Valid To,Active,Currently Valid");

        foreach (var d in paged.Items)
        {
            sb.AppendLine(string.Join(",",
                d.DiscountId,
                EscapeCsv(d.Name),
                EscapeCsv(d.DiscountType),
                EscapeCsv(d.ValueFormatted),
                EscapeCsv(d.ScopeType),
                EscapeCsv(d.ScopeLabel),
                d.MinQuantity,
                EscapeCsv(d.TargetSegment ?? "All"),
                EscapeCsv(d.CouponCode ?? "Auto"),
                d.UsedCount,
                d.MaxUses.HasValue ? d.MaxUses.Value.ToString() : "Unlimited",
                EscapeCsv(d.ValidFrom?.ToString("yyyy-MM-dd HH:mm") ?? "—"),
                EscapeCsv(d.ValidTo?.ToString("yyyy-MM-dd HH:mm") ?? "—"),
                d.IsActive,
                d.IsCurrentlyValid
            ));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"discount_rules_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
    }

    private static string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "\"\"";
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return $"\"{field}\"";
    }
}

using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Contracts;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Discounts;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[Route("api/discount-overrides")]
[ApiController]
[Authorize]
public class DiscountOverridesController : ControllerBase
{
    private readonly IDiscountOverrideService _overrideService;
    private readonly IRealTimeNotificationService _notifications;

    public DiscountOverridesController(
        IDiscountOverrideService overrideService,
        IRealTimeNotificationService notifications)
    {
        _overrideService = overrideService;
        _notifications = notifications;
    }

    [HttpGet("metrics")]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> GetMetrics(CancellationToken ct)
    {
        var metrics = await _overrideService.GetMetricsAsync(ct);
        return Ok(ApiResponse<DiscountOverrideMetricsDto>.Ok(metrics));
    }

    [HttpGet("paged")]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> GetPaged([FromQuery] DiscountOverrideFilterRequest request, CancellationToken ct)
    {
        var result = await _overrideService.GetOverridesPagedAsync(request, ct);
        return Ok(ApiResponse<PagedResult<DiscountOverrideDto>>.Ok(result));
    }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var list = await _overrideService.GetAllAsync(status);
        return Ok(ApiResponse<List<DiscountOverrideDto>>.Ok(list));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _overrideService.GetByIdAsync(id);
        if (dto is null)
            return NotFound(ApiErrorResponse.From("not_found", "Override request not found", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<DiscountOverrideDto>.Ok(dto));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.CashWrite)]
    public async Task<IActionResult> Create([FromBody] CreateDiscountOverrideRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var dto = await _overrideService.CreateAsync(request, userId);

        _ = _notifications.BroadcastNotificationAsync(new Store.Models.DTOs.Notifications.StoreNotificationDto
        {
            Title = "Discount Override Requested",
            Message = $"Cashier requested {dto.ValueFormatted} discount override ({dto.Justification ?? "No reason"}).",
            Category = Store.Models.DTOs.Notifications.NotificationCategory.DiscountApproval,
            Severity = "Warning",
            TargetUrl = "/DiscountOverrides",
            ActionLabel = "Review Request"
        });

        return CreatedAtAction(nameof(GetById), new { id = dto.DiscountOverrideRequestId },
            ApiResponse<DiscountOverrideDto>.Ok(dto));
    }

    [HttpPost("{id:int}/review")]
    [Authorize(Policy = PermissionKeys.PricingWrite)]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewDiscountOverrideRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var dto = await _overrideService.ReviewAsync(id, userId, request);
        if (dto is null)
            return BadRequest(ApiErrorResponse.From("bad_request",
                "Override request is not in Pending state or does not exist",
                traceId: HttpContext.TraceIdentifier));

        _ = _notifications.NotifyDiscountOverrideAsync(new Store.Models.DTOs.Notifications.DiscountOverrideNotificationDto
        {
            CashierUserId = dto.RequestedByUserId,
            SupervisorUserId = userId,
            Status = dto.Status,
            RequestedDiscount = dto.OverrideValue,
            Reason = dto.ReviewNotes,
            SupervisorName = dto.ReviewedByUser
        });

        return Ok(ApiResponse<DiscountOverrideDto>.Ok(dto));
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Policy = PermissionKeys.CashWrite)]
    public async Task<IActionResult> Cancel(int id)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var ok = await _overrideService.CancelAsync(id, userId);
        if (!ok)
            return BadRequest(ApiErrorResponse.From("bad_request",
                "Override request is not in Pending state or does not exist",
                traceId: HttpContext.TraceIdentifier));

        return NoContent();
    }

    [HttpGet("export/csv")]
    [Authorize(Policy = PermissionKeys.PricingRead)]
    public async Task<IActionResult> ExportCsv([FromQuery] DiscountOverrideFilterRequest request, CancellationToken ct)
    {
        request.Page = 1;
        request.PageSize = 2000;
        var paged = await _overrideService.GetOverridesPagedAsync(request, ct);

        var sb = new StringBuilder();
        sb.AppendLine("Request ID,Created Date,Scope,Target,Type,Value,Est Impact (XAF),Status,Cashier Username,Cashier Name,Supervisor,Review Date,Justification,Review Notes");

        foreach (var r in paged.Items)
        {
            sb.AppendLine(string.Join(",",
                r.DiscountOverrideRequestId,
                EscapeCsv(r.DateCreated.ToString("yyyy-MM-dd HH:mm")),
                EscapeCsv(r.ScopeType),
                EscapeCsv(r.ScopeLabel),
                EscapeCsv(r.OverrideType),
                EscapeCsv(r.ValueFormatted),
                r.EstimatedImpactXaf.ToString("F2"),
                EscapeCsv(r.Status),
                EscapeCsv(r.RequestedByUser),
                EscapeCsv(r.RequestedByFullName ?? ""),
                EscapeCsv(r.ReviewedByUser ?? "—"),
                EscapeCsv(r.ReviewedAt?.ToString("yyyy-MM-dd HH:mm") ?? "—"),
                EscapeCsv(r.Justification ?? ""),
                EscapeCsv(r.ReviewNotes ?? "")
            ));
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"discount_overrides_ledger_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
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

using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Contracts;
using Store.Models.DTOs.Cash;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[Route("api/cash/variances")]
[ApiController]
[Authorize]
public class CashVarianceController : ControllerBase
{
    private readonly ICashVarianceService _varianceService;

    public CashVarianceController(ICashVarianceService varianceService)
        => _varianceService = varianceService;

    [HttpGet("metrics")]
    [Authorize(Policy = PermissionKeys.CashRead)]
    public async Task<IActionResult> GetMetrics()
    {
        var metrics = await _varianceService.GetMetricsAsync();
        return Ok(ApiResponse<CashVarianceMetricsDto>.Ok(metrics));
    }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.CashRead)]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        CashVarianceStatus? parsed = null;
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<CashVarianceStatus>(status, ignoreCase: true, out var s))
            parsed = s;

        var list = await _varianceService.GetAllAsync(parsed);
        return Ok(ApiResponse<List<CashVarianceDto>>.Ok(list));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.CashRead)]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _varianceService.GetByIdAsync(id);
        if (dto is null)
            return NotFound(ApiErrorResponse.From("not_found", "Cash variance record not found",
                traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<CashVarianceDto>.Ok(dto));
    }

    [HttpGet("by-shift/{shiftId:guid}")]
    [Authorize(Policy = PermissionKeys.CashRead)]
    public async Task<IActionResult> GetByShift(Guid shiftId)
    {
        var list = await _varianceService.GetByShiftAsync(shiftId);
        return Ok(ApiResponse<List<CashVarianceDto>>.Ok(list));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.CashWrite)]
    public async Task<IActionResult> Record([FromBody] RecordCashVarianceRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var dto = await _varianceService.RecordAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = dto.CashVarianceRecordId },
            ApiResponse<CashVarianceDto>.Ok(dto));
    }

    [HttpPost("{id:int}/review")]
    [Authorize(Policy = PermissionKeys.CashWrite)]
    public async Task<IActionResult> Review(int id, [FromBody] ReviewCashVarianceRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var dto = await _varianceService.ReviewAsync(id, userId, request);
        if (dto is null)
            return BadRequest(ApiErrorResponse.From("bad_request",
                "Variance record must be in Pending status to review",
                traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<CashVarianceDto>.Ok(dto));
    }

    [HttpGet("export/csv")]
    [Authorize(Policy = PermissionKeys.CashRead)]
    public async Task<IActionResult> ExportCsv([FromQuery] string? status)
    {
        CashVarianceStatus? parsed = null;
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<CashVarianceStatus>(status, ignoreCase: true, out var s))
            parsed = s;

        var list = await _varianceService.GetAllAsync(parsed);
        var metrics = await _varianceService.GetMetricsAsync();
        var sb = new StringBuilder();

        sb.AppendLine($"# ClexAn Foods - Cash Variance & Float Audit Report ({DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC)");
        sb.AppendLine($"# Total Audits: {metrics.TotalRecords} | Pending: {metrics.TotalPendingCount} | Reviewed: {metrics.TotalReviewedCount} | Escalated: {metrics.TotalEscalatedCount}");
        sb.AppendLine($"# Net Discrepancy (XAF): {metrics.NetDiscrepancyXaf:N0} | Total Shortages: -{metrics.TotalShortagesXaf:N0} XAF | Total Overages: +{metrics.TotalOveragesXaf:N0} XAF");
        sb.AppendLine();
        sb.AppendLine("Record ID,Shift ID,Expected Amount (XAF),Actual Counted (XAF),Variance (XAF),Discrepancy Type,Reason Code,Status,Cashier User,Supervisor Reviewer,Review Notes,Reviewed At,Date Recorded");

        foreach (var v in list)
        {
            var discType = v.IsShortage ? "SHORTAGE" : v.IsOverage ? "OVERAGE" : "EXACT_MATCH";
            sb.AppendLine($"{v.CashVarianceRecordId},\"{v.CashierShiftId}\",{v.ExpectedAmount:N0},{v.ActualAmount:N0},{v.Variance:N0},{discType},\"{v.ReasonCode ?? "—"}\",{v.Status},\"{v.RecordedByUser}\",\"{v.ReviewedByUser ?? "—"}\",\"{v.ReviewNotes?.Replace("\"", "\"\"") ?? ""}\",\"{v.ReviewedAt:yyyy-MM-dd HH:mm}\",\"{v.DateCreated:yyyy-MM-dd HH:mm}\"");
        }

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"cash_variances_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
    }
}

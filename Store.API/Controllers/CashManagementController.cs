using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Audit;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/cash")]
[Authorize]
public class CashManagementController : ControllerBase
{
    private readonly IStoreOperationsService _ops;
    private readonly IAuditLogService _auditService;

    public CashManagementController(IStoreOperationsService ops, IAuditLogService auditService)
    {
        _ops = ops;
        _auditService = auditService;
    }

    [HttpGet("shift/active")]
    [Authorize(Policy = PermissionKeys.CashRead)]
    public async Task<IActionResult> GetActiveShift(CancellationToken ct)
    {
        if (!TryGetUserId(out var uid))
        {
            return Unauthorized();
        }

        var shift = await _ops.GetActiveShiftAsync(uid, ct);
        return shift is null ? NotFound() : Ok(shift);
    }

    [HttpPost("shift/open")]
    [Authorize(Policy = PermissionKeys.CashWrite)]
    public async Task<IActionResult> OpenShift([FromBody] ShiftOpenRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(out var uid))
        {
            return Unauthorized();
        }

        var shift = await _ops.OpenShiftAsync(request, uid, ct);

        // Record forensic audit log for shift opening
        await _auditService.LogAsync(new CreateAuditLogEntryRequest
        {
            UserId = uid,
            Action = "CashShift.Open",
            Category = "Financial",
            Severity = "Info",
            Summary = $"Opened cash drawer shift with starting float of {request.OpeningFloat:N0} XAF",
            TargetEntity = "CashShift",
            TargetId = shift.CashierShiftId.ToString(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
        }, ct);

        return Ok(shift);
    }

    [HttpPost("shift/close")]
    [Authorize(Policy = PermissionKeys.CashWrite)]
    public async Task<IActionResult> CloseShift([FromBody] ShiftCloseRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(out var uid))
        {
            return Unauthorized();
        }

        var shift = await _ops.CloseShiftAsync(request, uid, ct);
        if (shift == null) return NotFound();

        // Record forensic audit log for shift closure
        var variance = shift.VarianceAmount ?? 0m;
        await _auditService.LogAsync(new CreateAuditLogEntryRequest
        {
            UserId = uid,
            Action = "CashShift.Close",
            Category = "Financial",
            Severity = variance == 0 ? "Info" : (Math.Abs(variance) > 5000 ? "Warning" : "Info"),
            Summary = $"Closed cash drawer shift. Expected: {shift.ExpectedClosingAmount ?? 0:N0} XAF, Actual: {shift.ClosingFloat ?? 0:N0} XAF, Variance: {variance:N0} XAF",
            TargetEntity = "CashShift",
            TargetId = shift.CashierShiftId.ToString(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
        }, ct);

        return Ok(shift);
    }

    [HttpGet("shifts")]
    [Authorize(Policy = PermissionKeys.CashRead)]
    public async Task<IActionResult> GetShifts([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var shifts = await _ops.GetShiftsAsync(page, pageSize, ct);
        return Ok(shifts);
    }

    [HttpGet("report/z")]
    [Authorize(Policy = PermissionKeys.ReportsRead)]
    public async Task<IActionResult> DailyZReport([FromQuery] DateTime? dateUtc, CancellationToken ct)
    {
        var report = await _ops.GetDailyZReportAsync(dateUtc ?? DateTime.UtcNow, ct);
        return Ok(report);
    }

    [HttpGet("reconciliation")]
    [Authorize(Policy = PermissionKeys.ReportsRead)]
    public async Task<IActionResult> DayEndReconciliation([FromQuery] DateOnly? date, CancellationToken ct)
    {
        var reconciliation = await _ops.GetDayEndReconciliationAsync(date ?? DateOnly.FromDateTime(DateTime.UtcNow), ct);
        return Ok(reconciliation);
    }

    private bool TryGetUserId(out Guid uid)
    {
        var claim = User.FindFirst("uid")?.Value;
        return Guid.TryParse(claim, out uid);
    }
}

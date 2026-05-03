using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/cash")]
[Authorize]
public class CashManagementController : ControllerBase
{
    private readonly IStoreOperationsService _ops;

    public CashManagementController(IStoreOperationsService ops)
    {
        _ops = ops;
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
        return shift is null ? NotFound() : Ok(shift);
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

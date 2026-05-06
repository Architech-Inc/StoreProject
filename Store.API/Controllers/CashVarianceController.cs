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
}

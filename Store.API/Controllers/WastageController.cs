using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Inventory;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[Route("api/wastage")]
[ApiController]
[Authorize]
public class WastageController : ControllerBase
{
    private readonly IWastageService _wastageService;

    public WastageController(IWastageService wastageService)
        => _wastageService = wastageService;

    [HttpGet("metrics")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetMetrics(CancellationToken ct)
    {
        var metrics = await _wastageService.GetWastageMetricsAsync(ct);
        return Ok(ApiResponse<WastageMetricsDto>.Ok(metrics));
    }

    [HttpGet("paged")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetPaged([FromQuery] WastageFilterRequest request, CancellationToken ct)
    {
        var result = await _wastageService.GetWastagePagedAsync(request, ct);
        return Ok(ApiResponse<PagedResult<WastageEntryDto>>.Ok(result));
    }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? itemId, [FromQuery] string? wastageType)
    {
        var list = await _wastageService.GetAllAsync(itemId, wastageType);
        return Ok(ApiResponse<List<WastageEntryDto>>.Ok(list));
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _wastageService.GetByIdAsync(id);
        if (dto is null)
            return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Wastage entry not found", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<WastageEntryDto>.Ok(dto));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> Record([FromBody] RecordWastageRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var dto = await _wastageService.RecordAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = dto.WastageEntryId }, ApiResponse<WastageEntryDto>.Ok(dto));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var userIdClaim = User.FindFirst("uid")?.Value
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        Guid.TryParse(userIdClaim, out var deletedById);

        var ok = await _wastageService.DeleteAsync(id, deletedById == Guid.Empty ? null : deletedById, ct);
        if (!ok)
            return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Wastage entry not found", traceId: HttpContext.TraceIdentifier));
        return NoContent();
    }

    [HttpPost("{id:int}/restore")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> Restore(int id, CancellationToken ct = default)
    {
        var restored = await _wastageService.RestoreAsync(id, ct);
        if (!restored)
            return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Wastage entry not found or not deleted", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<object>.Ok(null!, "Wastage entry restored successfully."));
    }
}

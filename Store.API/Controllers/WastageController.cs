using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Contracts;
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
            return NotFound(ApiErrorResponse.From("not_found", "Wastage entry not found", traceId: HttpContext.TraceIdentifier));
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
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _wastageService.DeleteAsync(id);
        if (!ok)
            return NotFound(ApiErrorResponse.From("not_found", "Wastage entry not found", traceId: HttpContext.TraceIdentifier));
        return NoContent();
    }
}

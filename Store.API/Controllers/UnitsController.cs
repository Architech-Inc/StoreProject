using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Store.API.Attributes;
using Store.Models.Common;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Items;
using Store.Models.DTOs.Operations;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UnitsController : ControllerBase
{
    private readonly IUnitService _unitService;
    private readonly ILogger<UnitsController> _logger;

    public UnitsController(IUnitService unitService, ILogger<UnitsController> logger)
    {
        _unitService = unitService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<IEnumerable<Unit>>.Ok(await _unitService.GetAllAsync(ct)));

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var unit = await _unitService.GetByIdAsync(id, ct);
        if (unit is null)
            return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Unit not found."));

        return Ok(ApiResponse<Unit>.Ok(unit));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    [Audit("Create Unit", Category = "Inventory")]
    public async Task<IActionResult> Create([FromBody] CreateUnitRequest request, CancellationToken ct)
    {
        try
        {
            var unit = await _unitService.CreateAsync(request.Name, request.Abbreviation, request.Description, ct);
            return CreatedAtAction(nameof(GetById), new { id = unit.UnitId }, ApiResponse<Unit>.Ok(unit));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiErrorResponse.From(ErrorCode.Conflict, SafeErrorMessage.From(ex, _logger, "CreateUnit")));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    [Audit("Update Unit", Category = "Inventory")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUnitRequest request, CancellationToken ct)
    {
        try
        {
            var unit = await _unitService.UpdateAsync(id, request.Name, request.Abbreviation, request.Description, ct);
            if (unit is null)
                return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Unit not found."));

            return Ok(ApiResponse<Unit>.Ok(unit));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiErrorResponse.From(ErrorCode.Conflict, SafeErrorMessage.From(ex, _logger, "UpdateUnit")));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermissionKeys.AdminSystem)]
    [Audit("Delete Unit", Category = "Inventory")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            var userIdClaim = User.FindFirst("uid")?.Value;
            Guid.TryParse(userIdClaim, out var deletedById);

            var deleted = await _unitService.DeleteAsync(id, deletedById == Guid.Empty ? null : deletedById, ct);
            if (!deleted)
                return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Unit not found."));

            return Ok(ApiResponse<object>.Ok(null!, "Unit deleted."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiErrorResponse.From(ErrorCode.Conflict, SafeErrorMessage.From(ex, _logger, "DeleteUnit")));
        }
    }

    [HttpPost("{id:int}/restore")]
    [Authorize(Policy = PermissionKeys.AdminSystem)]
    [Audit("Restore Unit", Category = "Inventory")]
    public async Task<IActionResult> Restore(int id, CancellationToken ct)
    {
        var restored = await _unitService.RestoreAsync(id, ct);
        if (!restored)
            return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Unit not found or not deleted."));

        return Ok(ApiResponse<object>.Ok(null!, "Unit restored."));
    }
}

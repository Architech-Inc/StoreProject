using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Contracts;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Inventory;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class BatchesController : ControllerBase
{
    private readonly IBatchService _batchService;

    public BatchesController(IBatchService batchService)
        => _batchService = batchService;

    [HttpGet]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? itemId, [FromQuery] string? expiryStatus)
    {
        var list = await _batchService.GetAllAsync(itemId, expiryStatus);
        return Ok(ApiResponse<List<BatchDto>>.Ok(list));
    }

    [HttpGet("expiring")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetExpiring([FromQuery] int withinDays = 30)
    {
        var list = await _batchService.GetExpiringAsync(withinDays);
        return Ok(ApiResponse<List<BatchDto>>.Ok(list));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var dto = await _batchService.GetByIdAsync(id);
        if (dto is null)
            return NotFound(ApiErrorResponse.From("not_found", "Batch not found", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<BatchDto>.Ok(dto));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> Create([FromBody] CreateBatchRequest request)
    {
        var dto = await _batchService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = dto.BatchId }, ApiResponse<BatchDto>.Ok(dto));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBatchRequest request)
    {
        var dto = await _batchService.UpdateAsync(id, request);
        if (dto is null)
            return NotFound(ApiErrorResponse.From("not_found", "Batch not found", traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<BatchDto>.Ok(dto));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _batchService.DeleteAsync(id);
        if (!ok)
            return NotFound(ApiErrorResponse.From("not_found", "Batch not found", traceId: HttpContext.TraceIdentifier));
        return NoContent();
    }
}

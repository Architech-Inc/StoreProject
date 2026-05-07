using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Contracts;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;
using Store.Models.DTOs.Procurement;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[Route("api/suppliers")]
[ApiController]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
        => _supplierService = supplierService;

    [HttpGet]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetAll()
    {
        var suppliers = await _supplierService.GetAllAsync();
        return Ok(ApiResponse<List<SupplierDto>>.Ok(suppliers));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var supplier = await _supplierService.GetByIdAsync(id);
        if (supplier is null)
            return NotFound(ApiErrorResponse.From("not_found", "Supplier not found",
                traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<SupplierDto>.Ok(supplier));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.AdminBranches)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var supplier = await _supplierService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = supplier.SupplierId },
            ApiResponse<SupplierDto>.Ok(supplier));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionKeys.AdminBranches)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierRequest request)
    {
        var supplier = await _supplierService.UpdateAsync(id, request);
        if (supplier is null)
            return NotFound(ApiErrorResponse.From("not_found", "Supplier not found",
                traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<SupplierDto>.Ok(supplier));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionKeys.AdminBranches)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _supplierService.DeleteAsync(id);
        if (!success)
            return BadRequest(ApiErrorResponse.From("bad_request",
                "Supplier not found or has associated orders",
                traceId: HttpContext.TraceIdentifier));
        return NoContent();
    }
}

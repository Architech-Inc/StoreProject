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
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] string? city = null,
        [FromQuery] string? country = null,
        [FromQuery] string? sortBy = null)
    {
        var suppliers = await _supplierService.GetAllAsync(search, city, country, sortBy);
        return Ok(ApiResponse<List<SupplierDto>>.Ok(suppliers));
    }

    [HttpGet("paged")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetPaged([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await _supplierService.GetPagedAsync(request, ct);
        return Ok(ApiResponse<PagedResult<SupplierDto>>.Ok(result));
    }

    [HttpGet("metrics")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetMetrics()
    {
        var metrics = await _supplierService.GetMetricsAsync();
        return Ok(ApiResponse<SupplierMetricsDto>.Ok(metrics));
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

    [HttpGet("{id:guid}/profile")]
    [Authorize(Policy = PermissionKeys.InventoryRead)]
    public async Task<IActionResult> GetProfile(Guid id)
    {
        var profile = await _supplierService.GetProfileAsync(id);
        if (profile is null)
            return NotFound(ApiErrorResponse.From("not_found", "Supplier not found",
                traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<SupplierProfileDto>.Ok(profile));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request)
    {
        var userIdClaim = User.FindFirst("uid")?.Value 
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var supplier = await _supplierService.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = supplier.SupplierId },
            ApiResponse<SupplierDto>.Ok(supplier));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierRequest request)
    {
        var supplier = await _supplierService.UpdateAsync(id, request);
        if (supplier is null)
            return NotFound(ApiErrorResponse.From("not_found", "Supplier not found",
                traceId: HttpContext.TraceIdentifier));
        return Ok(ApiResponse<SupplierDto>.Ok(supplier));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionKeys.InventoryWrite)]
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

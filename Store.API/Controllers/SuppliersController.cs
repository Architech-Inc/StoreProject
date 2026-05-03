using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Contracts;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Suppliers;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var suppliers = await _supplierService.GetAllAsync(ct);
        var dtos = suppliers.Select(s => new SupplierDto
        {
            SupplierId = s.SupplierId,
            Name = s.Name,
            RegistrationNumber = s.RegistrationNumber,
            Notes = s.Notes
        });
        return Ok(ApiResponse<IEnumerable<SupplierDto>>.Ok(dtos));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var supplier = await _supplierService.GetByIdAsync(id, ct);
        if (supplier is null)
            return NotFound(ApiErrorResponse.From("not_found", "Supplier not found.", traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<SupplierDto>.Ok(new SupplierDto
        {
            SupplierId = supplier.SupplierId,
            Name = supplier.Name,
            RegistrationNumber = supplier.RegistrationNumber,
            Notes = supplier.Notes
        }));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var supplier = await _supplierService.CreateAsync(request.Name, request.RegistrationNumber, request.Notes, ct);
        var dto = new SupplierDto
        {
            SupplierId = supplier.SupplierId,
            Name = supplier.Name,
            RegistrationNumber = supplier.RegistrationNumber,
            Notes = supplier.Notes
        };
        return CreatedAtAction(nameof(GetById), new { id = supplier.SupplierId },
            ApiResponse<SupplierDto>.Ok(dto, "Supplier created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var supplier = await _supplierService.UpdateAsync(id, request.Name, request.RegistrationNumber, request.Notes, ct);
        if (supplier is null)
            return NotFound(ApiErrorResponse.From("not_found", "Supplier not found.", traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<SupplierDto>.Ok(new SupplierDto
        {
            SupplierId = supplier.SupplierId,
            Name = supplier.Name,
            RegistrationNumber = supplier.RegistrationNumber,
            Notes = supplier.Notes
        }));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _supplierService.DeleteAsync(id, ct);
        if (!deleted)
            return NotFound(ApiErrorResponse.From("not_found", "Supplier not found.", traceId: HttpContext.TraceIdentifier));

        return Ok(ApiResponse<object>.Ok(null!, "Supplier deleted."));
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Employees;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;

using Store.API.Attributes;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PermissionKeys.EmployeeRead)]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService) => _employeeService = employeeService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EmployeeFilterRequest request, CancellationToken ct)
    {
        var result = await _employeeService.GetAllAsync(request, ct);
        return Ok(ApiResponse<PagedResult<EmployeeDto>>.Ok(result));
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics(CancellationToken ct)
    {
        var metrics = await _employeeService.GetMetricsAsync(ct);
        return Ok(ApiResponse<EmployeeMetricsDto>.Ok(metrics));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var employee = await _employeeService.GetByIdAsync(id, ct);
        if (employee is null) return NotFound(ApiResponse<object>.Fail("Employee not found."));
        return Ok(ApiResponse<EmployeeDto>.Ok(employee));
    }

    [HttpGet("{id:guid}/360")]
    public async Task<IActionResult> Get360ById(Guid id, CancellationToken ct)
    {
        var employee360 = await _employeeService.Get360ByIdAsync(id, ct);
        if (employee360 is null) return NotFound(ApiResponse<object>.Fail("Employee not found."));
        return Ok(ApiResponse<Employee360Dto>.Ok(employee360));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.EmployeeCreate)]
    [Audit("Create Employee", Category = "HR")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request, CancellationToken ct)
    {
        var employee = await _employeeService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = employee.EmployeeId }, ApiResponse<EmployeeDto>.Ok(employee, "Employee created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionKeys.EmployeeUpdate)]
    [Audit("Update Employee", Category = "HR")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequest request, CancellationToken ct)
    {
        var employee = await _employeeService.UpdateAsync(id, request, ct);
        if (employee is null) return NotFound(ApiResponse<object>.Fail("Employee not found."));
        return Ok(ApiResponse<EmployeeDto>.Ok(employee));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionKeys.EmployeeDelete)]
    [Audit("Delete Employee", Category = "HR")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("uid")?.Value;
        Guid.TryParse(userIdClaim, out var deletedById);

        var deleted = await _employeeService.DeleteAsync(id, deletedById == Guid.Empty ? null : deletedById, ct);
        if (!deleted) return NotFound(ApiResponse<object>.Fail("Employee not found."));
        return Ok(ApiResponse<object>.Ok(null!, "Employee removed."));
    }

    [HttpPost("{id:guid}/restore")]
    [Authorize(Policy = PermissionKeys.EmployeeUpdate)]
    [Audit("Restore Employee", Category = "HR")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        var restored = await _employeeService.RestoreAsync(id, ct);
        if (!restored) return NotFound(ApiResponse<object>.Fail("Employee not found or not deleted."));
        return Ok(ApiResponse<object>.Ok(null!, "Employee restored."));
    }
}

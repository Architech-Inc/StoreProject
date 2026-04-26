using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Employees;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService) => _employeeService = employeeService;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await _employeeService.GetAllAsync(request, ct);
        return Ok(ApiResponse<PagedResult<EmployeeDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var employee = await _employeeService.GetByIdAsync(id, ct);
        if (employee is null) return NotFound(ApiResponse<object>.Fail("Employee not found."));
        return Ok(ApiResponse<EmployeeDto>.Ok(employee));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request, CancellationToken ct)
    {
        var employee = await _employeeService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = employee.EmployeeId }, ApiResponse<EmployeeDto>.Ok(employee, "Employee created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequest request, CancellationToken ct)
    {
        var employee = await _employeeService.UpdateAsync(id, request, ct);
        if (employee is null) return NotFound(ApiResponse<object>.Fail("Employee not found."));
        return Ok(ApiResponse<EmployeeDto>.Ok(employee));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _employeeService.DeleteAsync(id, ct);
        if (!deleted) return NotFound(ApiResponse<object>.Fail("Employee not found."));
        return Ok(ApiResponse<object>.Ok(null!, "Employee removed."));
    }
}

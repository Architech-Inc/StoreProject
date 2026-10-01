using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Store.API.Attributes;
using Store.Models.Common;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.HR;
using Store.Models.DTOs.Operations;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _deptService;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(IDepartmentService deptService, ILogger<DepartmentsController> logger)
    {
        _deptService = deptService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<IEnumerable<Department>>.Ok(await _deptService.GetAllAsync(ct)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var dept = await _deptService.GetByIdAsync(id, ct);
        if (dept is null)
            return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Department not found."));

        return Ok(ApiResponse<Department>.Ok(dept));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.EmployeeCreate)]
    [Audit("Create Department", Category = "HR")]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request, CancellationToken ct)
    {
        try
        {
            var dept = await _deptService.CreateAsync(request.Name, request.Description, ct);
            return CreatedAtAction(nameof(GetById), new { id = dept.DepartmentId }, ApiResponse<Department>.Ok(dept));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiErrorResponse.From(ErrorCode.Conflict, SafeErrorMessage.From(ex, _logger, "CreateDepartment")));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.EmployeeUpdate)]
    [Audit("Update Department", Category = "HR")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDepartmentRequest request, CancellationToken ct)
    {
        try
        {
            var dept = await _deptService.UpdateAsync(id, request.Name, request.Description, ct);
            if (dept is null)
                return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Department not found."));

            return Ok(ApiResponse<Department>.Ok(dept));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiErrorResponse.From(ErrorCode.Conflict, SafeErrorMessage.From(ex, _logger, "UpdateDepartment")));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermissionKeys.AdminSystem)]
    [Audit("Delete Department", Category = "HR")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            var deleted = await _deptService.DeleteAsync(id, ct);
            if (!deleted)
                return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Department not found."));

            return Ok(ApiResponse<object>.Ok(null!, "Department deleted."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiErrorResponse.From(ErrorCode.Conflict, SafeErrorMessage.From(ex, _logger, "DeleteDepartment")));
        }
    }
}

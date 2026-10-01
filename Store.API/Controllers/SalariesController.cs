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
public class SalariesController : ControllerBase
{
    private readonly ISalaryService _salaryService;
    private readonly ILogger<SalariesController> _logger;

    public SalariesController(ISalaryService salaryService, ILogger<SalariesController> logger)
    {
        _salaryService = salaryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(ApiResponse<IEnumerable<Salary>>.Ok(await _salaryService.GetAllAsync(ct)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var salary = await _salaryService.GetByIdAsync(id, ct);
        if (salary is null)
            return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Salary grade not found."));

        return Ok(ApiResponse<Salary>.Ok(salary));
    }

    [HttpPost]
    [Authorize(Policy = PermissionKeys.PayrollWrite)]
    [Audit("Create Salary Grade", Category = "HR")]
    public async Task<IActionResult> Create([FromBody] CreateSalaryRequest request, CancellationToken ct)
    {
        try
        {
            var salary = await _salaryService.CreateAsync(request.Grade, request.BasicAmount, request.AllowanceAmount, request.Description, ct);
            return CreatedAtAction(nameof(GetById), new { id = salary.SalaryId }, ApiResponse<Salary>.Ok(salary));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiErrorResponse.From(ErrorCode.Conflict, SafeErrorMessage.From(ex, _logger, "CreateSalary")));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = PermissionKeys.PayrollWrite)]
    [Audit("Update Salary Grade", Category = "HR")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSalaryRequest request, CancellationToken ct)
    {
        try
        {
            var salary = await _salaryService.UpdateAsync(id, request.Grade, request.BasicAmount, request.AllowanceAmount, request.Description, ct);
            if (salary is null)
                return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Salary grade not found."));

            return Ok(ApiResponse<Salary>.Ok(salary));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiErrorResponse.From(ErrorCode.Conflict, SafeErrorMessage.From(ex, _logger, "UpdateSalary")));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = PermissionKeys.AdminSystem)]
    [Audit("Delete Salary Grade", Category = "HR")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            var deleted = await _salaryService.DeleteAsync(id, ct);
            if (!deleted)
                return NotFound(ApiErrorResponse.From(ErrorCode.NotFound, "Salary grade not found."));

            return Ok(ApiResponse<object>.Ok(null!, "Salary grade deleted."));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiErrorResponse.From(ErrorCode.Conflict, SafeErrorMessage.From(ex, _logger, "DeleteSalary")));
        }
    }
}

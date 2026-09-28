using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.DbServices.Services.Interfaces;
using Store.Models.DTOs.Operations;
using Store.Models.Entities.HR;
using Store.Models.Enums;

using Microsoft.Extensions.Logging;
using Store.Models.Common;
namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PermissionKeys.AdminSettings)] // Or a dedicated HR/Finance policy
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _payrollService;
    private readonly ILogger<PayrollController> _logger;

    public PayrollController(IPayrollService payrollService, ILogger<PayrollController> logger)
    {
        _payrollService = payrollService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRuns()
    {
        var runs = await _payrollService.GetAllPayrollRunsAsync();
        return Ok(ApiResponse<IEnumerable<PayrollRun>>.Ok(runs, "Payroll runs retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRunById(Guid id)
    {
        var run = await _payrollService.GetPayrollRunByIdAsync(id);
        if (run == null)
            return NotFound(ApiResponse.Fail("Payroll run not found."));

        return Ok(ApiResponse<PayrollRun>.Ok(run, "Payroll run retrieved successfully."));
    }

    [HttpGet("{id:guid}/payslips")]
    public async Task<IActionResult> GetPayslips(Guid id)
    {
        var payslips = await _payrollService.GetPayslipsForRunAsync(id);
        return Ok(ApiResponse<IEnumerable<Payslip>>.Ok(payslips, "Payslips retrieved successfully."));
    }

    public class DraftPayrollRequest
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
    }

    [HttpPost("draft")]
    public async Task<IActionResult> DraftPayrollRun([FromBody] DraftPayrollRequest request)
    {
        try
        {
            var run = await _payrollService.DraftPayrollRunAsync(request.PeriodStart, request.PeriodEnd);
            return Ok(ApiResponse<PayrollRun>.Ok(run, "Payroll run drafted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(SafeErrorMessage.From(ex, _logger, "Payroll operation")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to draft payroll run.");
            return StatusCode(500, ApiResponse.Fail("An error occurred while drafting the payroll run."));
        }
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApprovePayroll(Guid id)
    {
        try
        {
            var userIdString = User.FindFirst("uid")?.Value;
            if (!Guid.TryParse(userIdString, out var userId))
                return Unauthorized(ApiResponse.Fail("Invalid user identity."));

            var run = await _payrollService.ApprovePayrollAsync(id, userId);
            return Ok(ApiResponse<PayrollRun>.Ok(run, "Payroll run approved successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(SafeErrorMessage.From(ex, _logger, "Payroll operation")));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse.Fail("Payroll run not found."));
        }
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> PayPayroll(Guid id)
    {
        try
        {
            var success = await _payrollService.PayPayrollRunAsync(id);
            return Ok(ApiResponse.Ok("Payroll run paid and journal entry posted successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(SafeErrorMessage.From(ex, _logger, "Payroll operation")));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse.Fail("Payroll run not found."));
        }
    }
}

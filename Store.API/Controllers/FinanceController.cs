using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Finance;
using Store.Models.Interfaces.Services;

using Store.Models.DTOs.Operations;

namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = PermissionKeys.FinanceRead)]
public class FinanceController : ControllerBase
{
    private readonly IFinanceService _financeService;

    public FinanceController(IFinanceService financeService)
    {
        _financeService = financeService;
    }

    [HttpGet("profit-and-loss")]
    public async Task<ActionResult<ProfitAndLossDto>> GetProfitAndLoss([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, CancellationToken ct)
    {
        var start = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var end = endDate ?? DateTime.UtcNow;

        var result = await _financeService.GenerateProfitAndLossAsync(start, end, ct);
        return Ok(result);
    }

    [HttpGet("balance-sheet")]
    public async Task<ActionResult<BalanceSheetDto>> GetBalanceSheet([FromQuery] DateTime? asOfDate, CancellationToken ct)
    {
        var date = asOfDate ?? DateTime.UtcNow;

        var result = await _financeService.GenerateBalanceSheetAsync(date, ct);
        return Ok(result);
    }

    [HttpGet("ar-aging")]
    public async Task<ActionResult<AgingReportDto>> GetAccountsReceivableAging([FromQuery] DateTime? asOfDate, CancellationToken ct)
    {
        var date = asOfDate ?? DateTime.UtcNow;
        var result = await _financeService.GetAccountsReceivableAgingAsync(date, ct);
        return Ok(result);
    }

    [HttpGet("ap-aging")]
    public async Task<ActionResult<AgingReportDto>> GetAccountsPayableAging([FromQuery] DateTime? asOfDate, CancellationToken ct)
    {
        var date = asOfDate ?? DateTime.UtcNow;
        var result = await _financeService.GetAccountsPayableAgingAsync(date, ct);
        return Ok(result);
    }
}

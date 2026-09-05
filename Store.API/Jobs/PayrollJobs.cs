using Store.DbServices.Services.Interfaces;

namespace Store.API.Jobs;

public class PayrollJobs
{
    private readonly IPayrollService _payrollService;
    private readonly ILogger<PayrollJobs> _logger;

    public PayrollJobs(IPayrollService payrollService, ILogger<PayrollJobs> logger)
    {
        _payrollService = payrollService;
        _logger = logger;
    }

    /// <summary>
    /// Automatically drafts a payroll run for the current month.
    /// Expected to be scheduled to run on the 28th of every month.
    /// </summary>
    public async Task DraftMonthlyPayrollAsync()
    {
        try
        {
            _logger.LogInformation("Starting automated monthly payroll draft.");

            var today = DateTime.UtcNow;
            var periodStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var periodEnd = periodStart.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

            var run = await _payrollService.DraftPayrollRunAsync(periodStart, periodEnd);

            _logger.LogInformation("Successfully drafted automated monthly payroll run {Id} for period {Start} to {End}",
                run.PayrollRunId, periodStart, periodEnd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to automatically draft monthly payroll.");
            throw; // Hangfire will retry or mark as failed
        }
    }
}

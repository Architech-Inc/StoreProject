using Microsoft.AspNetCore.Mvc;
using Store.Models.Entities.HR;
using Store.Models.Entities.HR;
using StoreUI.Pages;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class PayrollModel : SecurePageModel
{
    private readonly IPayrollManager _payrollManager;
    private readonly IApiClientService _apiClient;

    public PayrollModel(IPayrollManager payrollManager, IApiClientService apiClient)
    {
        _payrollManager = payrollManager;
        _apiClient = apiClient;
    }

    public IEnumerable<PayrollRun> PayrollRuns { get; private set; } = new List<PayrollRun>();

    [BindProperty]
    public DateTime DraftPeriodStart { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    
    [BindProperty]
    public DateTime DraftPeriodEnd { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        PayrollRuns = await _payrollManager.GetAllRunsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDraftAsync()
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        var response = await _payrollManager.DraftPayrollAsync(DraftPeriodStart, DraftPeriodEnd);
        if (response.Success)
        {
            StatusMessage = "Payroll drafted successfully.";
        }
        else
        {
            StatusMessage = $"Error: {response.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid runId)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        var response = await _payrollManager.ApprovePayrollAsync(runId);
        if (response.Success)
        {
            StatusMessage = "Payroll approved successfully.";
        }
        else
        {
            StatusMessage = $"Error: {response.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostPayAsync(Guid runId)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        var response = await _payrollManager.PayPayrollAsync(runId);
        if (response.Success)
        {
            StatusMessage = "Payroll paid successfully.";
        }
        else
        {
            StatusMessage = $"Error: {response.Message}";
        }

        return RedirectToPage();
    }
}

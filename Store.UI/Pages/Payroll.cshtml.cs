using Microsoft.AspNetCore.Mvc;
using Store.Models.Entities.HR;
using Store.Models.DTOs.HR;
using Store.Models.DTOs.Operations;
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
    public IEnumerable<TaxBracketDto> TaxBrackets { get; private set; } = new List<TaxBracketDto>();

    // UX-05 — payroll approval + tax-bracket deletion are admin actions.
    public bool CanApprove { get; private set; }
    public bool CanDelete { get; private set; }

    [BindProperty]
    public DateTime DraftPeriodStart { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    
    [BindProperty]
    public DateTime DraftPeriodEnd { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!TryGetSecurityContext(out var token, out var permissions)) return GoToLogin();
        _apiClient.SetToken(token);

        // UX-05 — payroll approval + tax-bracket deletion both require
        // AdminUsers. Anyone with PayrollRead/PayrollWrite can view; only
        // admins can act on destructive operations.
        CanApprove = HasPermission(permissions, PermissionKeys.AdminUsers);
        CanDelete = HasPermission(permissions, PermissionKeys.AdminUsers);

        var runsTask = _payrollManager.GetAllRunsAsync();
        var bracketsTask = _payrollManager.GetTaxBracketsAsync();

        await Task.WhenAll(runsTask, bracketsTask);

        PayrollRuns = await runsTask;
        TaxBrackets = await bracketsTask;
        return Page();
    }

    public async Task<IActionResult> OnPostDraftAsync()
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        var response = await _payrollManager.DraftPayrollAsync(DraftPeriodStart, DraftPeriodEnd);
        if (response.Success)
        {
            StatusMessage = !string.IsNullOrWhiteSpace(response.Message) ? response.Message : "Payroll drafted successfully.";
        }
        else
        {
            StatusMessage = $"Error: {(!string.IsNullOrWhiteSpace(response.Message) ? response.Message : "Failed to draft payroll run.")}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid runId)
    {
        if (!TryGetSecurityContext(out var token, out var permissions)) return GoToLogin();

        // UX-05 — server-side gate. Payroll approval triggers journal
        // entries; that's an admin action.
        if (!HasPermission(permissions, PermissionKeys.AdminUsers))
        {
            StatusMessage = "Error: approving a payroll run requires administrator privileges.";
            return RedirectToPage();
        }

        _apiClient.SetToken(token);

        var response = await _payrollManager.ApprovePayrollAsync(runId);
        if (response.Success)
        {
            StatusMessage = !string.IsNullOrWhiteSpace(response.Message) ? response.Message : "Payroll approved successfully.";
        }
        else
        {
            StatusMessage = $"Error: {(!string.IsNullOrWhiteSpace(response.Message) ? response.Message : "Failed to approve payroll run.")}";
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
            StatusMessage = !string.IsNullOrWhiteSpace(response.Message) ? response.Message : "Payroll paid successfully.";
        }
        else
        {
            StatusMessage = $"Error: {(!string.IsNullOrWhiteSpace(response.Message) ? response.Message : "Failed to pay payroll run.")}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveTaxBracketAsync(int id, decimal minAmount, decimal? maxAmount, decimal taxPercentage, decimal fixedTaxAmount, bool isActive = true, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        var response = await _payrollManager.SaveTaxBracketAsync(id, minAmount, maxAmount, taxPercentage, fixedTaxAmount, isActive, ct);
        if (response.Success)
        {
            StatusMessage = id == 0 ? "Tax bracket created successfully." : "Tax bracket updated successfully.";
        }
        else
        {
            StatusMessage = $"Error: {(!string.IsNullOrWhiteSpace(response.Message) ? response.Message : "Failed to save tax bracket.")}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteTaxBracketAsync(int id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions)) return GoToLogin();

        // UX-05 — server-side gate. Tax brackets are global config;
        // only admins should mutate them.
        if (!HasPermission(permissions, PermissionKeys.AdminUsers))
        {
            StatusMessage = "Error: deleting tax brackets requires administrator privileges.";
            return RedirectToPage();
        }

        _apiClient.SetToken(token);

        var ok = await _payrollManager.DeleteTaxBracketAsync(id, ct);
        if (ok)
        {
            StatusMessage = "Tax bracket deleted successfully.";
        }
        else
        {
            StatusMessage = "Error: Could not delete tax bracket.";
        }

        return RedirectToPage();
    }
}

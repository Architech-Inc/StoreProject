using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using StoreUI.Services;

namespace StoreUI.Pages;

public class ReconciliationModel : SecurePageModel
{
    private readonly IReconciliationManager _reconciliationManager;
    private readonly IApiClientService _apiClient;

    public DayEndReconciliationDto? Report { get; private set; }
    public string ActiveTab { get; private set; } = "shifts";
    public string ActivePreset { get; private set; } = "today";
    public DateOnly SelectedDate { get; private set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public string? ErrorMessage { get; private set; }

    public ReconciliationModel(IReconciliationManager reconciliationManager, IApiClientService apiClient)
    {
        _reconciliationManager = reconciliationManager;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(
        [FromQuery] DateOnly? date,
        [FromQuery] string? tab = null,
        [FromQuery] string? preset = null,
        CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        if (!HasPermission(permissions, PermissionKeys.ReportsRead) &&
            !HasPermission(permissions, PermissionKeys.CashRead))
            return AccessDenied();

        _apiClient.SetToken(token);

        ActiveTab = string.IsNullOrWhiteSpace(tab) ? "shifts" : tab.Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(preset))
        {
            ActivePreset = preset.Trim().ToLowerInvariant();
            if (ActivePreset == "yesterday")
            {
                SelectedDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
            }
            else
            {
                SelectedDate = DateOnly.FromDateTime(DateTime.UtcNow);
                ActivePreset = "today";
            }
        }
        else if (date.HasValue)
        {
            SelectedDate = date.Value;
            ActivePreset = (SelectedDate == DateOnly.FromDateTime(DateTime.UtcNow)) ? "today" :
                           (SelectedDate == DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))) ? "yesterday" : "custom";
        }

        ViewData["Title"] = "Day-End Reconciliation";
        ViewData["ActivePage"] = "DayEndReconciliation";
        ViewData["PageDescription"] = "Consolidated register balance audits, payment tender reconciliations, and supervisory day-end sign-offs.";

        try
        {
            Report = await _reconciliationManager.GetDayEndReconciliationAsync(SelectedDate, ct);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load day-end reconciliation data: {ex.Message}";
        }

        return Page();
    }

    public async Task<IActionResult> OnGetExportCsvAsync(
        [FromQuery] DateOnly? date,
        CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return Unauthorized();

        if (!HasPermission(permissions, PermissionKeys.ReportsRead) && !HasPermission(permissions, PermissionKeys.CashRead))
            return Forbid();

        _apiClient.SetToken(token);
        var targetDate = date ?? SelectedDate;
        var report = await _reconciliationManager.GetDayEndReconciliationAsync(targetDate, ct);

        if (report is null)
            return NotFound("No reconciliation report available for this date.");

        var csvBytes = _reconciliationManager.GenerateReconciliationCsv(report);
        return File(csvBytes, "text/csv", $"day_end_reconciliation_{targetDate:yyyyMMdd}.csv");
    }
}

using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using StoreUI.Services;

namespace StoreUI.Pages;

public class CashReportsModel : SecurePageModel
{
    private readonly ICashReportsManager _reportsManager;
    private readonly IApiClientService _apiClient;

    public CashierShiftDto? ActiveShift { get; private set; }
    public DailyZReportDto? Report { get; private set; }

    public string ActiveTab { get; private set; } = "zreport";
    public string ActivePreset { get; private set; } = "today";

    [BindProperty(SupportsGet = true)]
    public DateTime ReportDateUtc { get; set; } = DateTime.UtcNow.Date;

    [BindProperty] public decimal OpeningFloat { get; set; }
    [BindProperty] public string? OpenNotes { get; set; }

    [BindProperty] public decimal ClosingFloat { get; set; }
    [BindProperty] public string? CloseNotes { get; set; }

    public bool CanCashRead { get; private set; }
    public bool CanCashWrite { get; private set; }
    public bool CanReportRead { get; private set; }

    [TempData] public string? StatusMessage { get; set; }

    public CashReportsModel(ICashReportsManager reportsManager, IApiClientService apiClient)
    {
        _reportsManager = reportsManager;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(
        [FromQuery] string? tab = null,
        [FromQuery] string? preset = null,
        [FromQuery] DateTime? date = null,
        CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        _apiClient.SetToken(token);
        CanCashRead = HasPermission(permissions, PermissionKeys.CashRead);
        CanCashWrite = HasPermission(permissions, PermissionKeys.CashWrite);
        CanReportRead = HasPermission(permissions, PermissionKeys.ReportsRead);

        if (!CanCashRead && !CanReportRead)
            return AccessDenied();

        ActiveTab = string.IsNullOrWhiteSpace(tab) ? "zreport" : tab.Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(preset))
        {
            ActivePreset = preset.Trim().ToLowerInvariant();
            if (ActivePreset == "yesterday")
            {
                ReportDateUtc = DateTime.UtcNow.Date.AddDays(-1);
            }
            else
            {
                ReportDateUtc = DateTime.UtcNow.Date;
                ActivePreset = "today";
            }
        }
        else if (date.HasValue)
        {
            ReportDateUtc = date.Value.Date;
            ActivePreset = (ReportDateUtc == DateTime.UtcNow.Date) ? "today" :
                           (ReportDateUtc == DateTime.UtcNow.Date.AddDays(-1)) ? "yesterday" : "custom";
        }

        ViewData["Title"] = "Cash & Shift Reports / Z-Reports";
        ViewData["ActivePage"] = "CashReports";
        ViewData["PageDescription"] = "POS register shift lifecycle management, daily Z-report fiscal summaries, and printable end-of-day reconciliation vouchers.";

        await LoadDataAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostOpenShiftAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        if (!HasPermission(permissions, PermissionKeys.CashWrite))
            return AccessDenied();

        _apiClient.SetToken(token);

        try
        {
            var req = new ShiftOpenRequest { OpeningFloat = OpeningFloat, Notes = OpenNotes };
            var shift = await _reportsManager.OpenShiftAsync(req, ct);
            StatusMessage = shift is not null
                ? $"Register shift #{shift.CashierShiftId.ToString()[..8]} successfully opened with {shift.OpeningFloat:N0} XAF float."
                : "Error: Unable to open shift. You may already have an active shift.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Failed to open shift: {ex.Message}";
        }

        return RedirectToPage(new { tab = "shift" });
    }

    public async Task<IActionResult> OnPostCloseShiftAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        if (!HasPermission(permissions, PermissionKeys.CashWrite))
            return AccessDenied();

        _apiClient.SetToken(token);

        try
        {
            var req = new ShiftCloseRequest { ClosingFloat = ClosingFloat, Notes = CloseNotes };
            var shift = await _reportsManager.CloseShiftAsync(req, ct);
            StatusMessage = shift is not null
                ? $"Register shift #{shift.CashierShiftId.ToString()[..8]} closed. Count: {shift.ClosingFloat:N0} XAF | Variance: {(shift.VarianceAmount >= 0 ? "+" : "")}{shift.VarianceAmount:N0} XAF."
                : "Error: No active shift found to close.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Failed to close shift: {ex.Message}";
        }

        return RedirectToPage(new { tab = "shift" });
    }

    public async Task<IActionResult> OnGetExportCsvAsync(
        [FromQuery] DateTime? date = null,
        CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return Unauthorized();

        if (!HasPermission(permissions, PermissionKeys.ReportsRead) && !HasPermission(permissions, PermissionKeys.CashRead))
            return Forbid();

        _apiClient.SetToken(token);
        var targetDate = date?.Date ?? ReportDateUtc.Date;
        var report = await _reportsManager.GetDailyZReportAsync(targetDate, ct);

        if (report is null)
            return NotFound("No Z-Report data available for this date.");

        var csvBytes = _reportsManager.GenerateZReportCsv(report);
        return File(csvBytes, "text/csv", $"z_report_{targetDate:yyyyMMdd}.csv");
    }

    private async Task LoadDataAsync(CancellationToken ct)
    {
        if (CanCashRead)
        {
            ActiveShift = await _reportsManager.GetActiveShiftAsync(ct);
        }

        if (CanReportRead)
        {
            Report = await _reportsManager.GetDailyZReportAsync(ReportDateUtc, ct);
        }
    }
}

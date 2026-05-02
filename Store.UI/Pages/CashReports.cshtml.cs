using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using StoreUI.Services;

namespace StoreUI.Pages;

public class CashReportsModel : SecurePageModel
{
    private readonly IApiClientService _apiClient;

    public CashierShiftDto? ActiveShift { get; private set; }
    public DailyZReportDto? Report { get; private set; }

    [BindProperty] public decimal OpeningFloat { get; set; }
    [BindProperty] public string? OpenNotes { get; set; }

    [BindProperty] public decimal ClosingFloat { get; set; }
    [BindProperty] public string? CloseNotes { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime ReportDateUtc { get; set; } = DateTime.UtcNow.Date;

    public bool CanCashRead { get; private set; }
    public bool CanCashWrite { get; private set; }
    public bool CanReportRead { get; private set; }

    [TempData] public string? StatusMessage { get; set; }

    public CashReportsModel(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
        {
            return GoToLogin();
        }

        _apiClient.SetToken(token);
        CanCashRead = HasPermission(permissions, PermissionKeys.CashRead);
        CanCashWrite = HasPermission(permissions, PermissionKeys.CashWrite);
        CanReportRead = HasPermission(permissions, PermissionKeys.ReportsRead);

        if (!CanCashRead && !CanReportRead)
        {
            return Forbid();
        }

        await LoadAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostOpenShiftAsync(CancellationToken ct)
    {
        return await HandleCashWriteAsync(async () =>
        {
            var req = new ShiftOpenRequest { OpeningFloat = OpeningFloat, Notes = OpenNotes };
            var shift = await _apiClient.PostAsync<CashierShiftDto>("/api/cash/shift/open", req, ct);
            StatusMessage = shift is null ? "Unable to open shift." : "Shift opened.";
        });
    }

    public async Task<IActionResult> OnPostCloseShiftAsync(CancellationToken ct)
    {
        return await HandleCashWriteAsync(async () =>
        {
            var req = new ShiftCloseRequest { ClosingFloat = ClosingFloat, Notes = CloseNotes };
            var shift = await _apiClient.PostAsync<CashierShiftDto>("/api/cash/shift/close", req, ct);
            StatusMessage = shift is null ? "No active shift found." : "Shift closed.";
        });
    }

    private async Task<IActionResult> HandleCashWriteAsync(Func<Task> operation)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
        {
            return GoToLogin();
        }

        _apiClient.SetToken(token);
        if (!HasPermission(permissions, PermissionKeys.CashWrite))
        {
            return Forbid();
        }

        await operation();
        return RedirectToPage(new { ReportDateUtc = ReportDateUtc.ToString("yyyy-MM-dd") });
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        ActiveShift = await _apiClient.GetAsync<CashierShiftDto>("/api/cash/shift/active", ct);
        Report = await _apiClient.GetAsync<DailyZReportDto>($"/api/cash/report/z?dateUtc={ReportDateUtc:O}", ct);
    }
}

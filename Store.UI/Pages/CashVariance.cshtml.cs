using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Cash;
using Store.Models.DTOs.Operations;
using Store.Models.Enums;
using StoreUI.Services;

namespace StoreUI.Pages;

public class CashVarianceModel : SecurePageModel
{
    private readonly ICashVarianceManager _varianceManager;
    private readonly IApiClientService _apiClient;

    public List<CashVarianceDto> Variances { get; private set; } = new();
    public CashVarianceMetricsDto Metrics { get; private set; } = new();
    public string ActiveTab { get; private set; } = "all";
    public string? FilterStatus { get; private set; }
    public string? SearchQuery { get; private set; }

    // ---- Record Variance ----
    [BindProperty] public Guid RecordShiftId { get; set; }
    [BindProperty] public decimal RecordExpected { get; set; }
    [BindProperty] public decimal RecordActual { get; set; }
    [BindProperty] public string? RecordReasonCode { get; set; }
    [BindProperty] public string? RecordNotes { get; set; }

    // ---- Review Variance ----
    [BindProperty] public int ReviewVarianceId { get; set; }
    [BindProperty] public CashVarianceStatus ReviewStatus { get; set; } = CashVarianceStatus.Reviewed;
    [BindProperty] public string? ReviewNotes { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public IEnumerable<CashVarianceStatus> ReviewableStatuses { get; } =
        new[] { CashVarianceStatus.Reviewed, CashVarianceStatus.Escalated, CashVarianceStatus.Pending };

    public CashVarianceModel(
        ICashVarianceManager varianceManager,
        IApiClientService apiClient)
    {
        _varianceManager = varianceManager;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(
        [FromQuery] string? status = null,
        [FromQuery] string? tab = null,
        [FromQuery] string? q = null,
        CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        if (!HasPermission(permissions, PermissionKeys.CashRead) && !HasPermission(permissions, PermissionKeys.ReportsRead))
            return AccessDenied();

        _apiClient.SetToken(token);

        ActiveTab = string.IsNullOrWhiteSpace(tab) ? "all" : tab.Trim().ToLowerInvariant();
        FilterStatus = status;
        SearchQuery = q;

        CashVarianceStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CashVarianceStatus>(status, true, out var s))
        {
            parsedStatus = s;
        }

        Metrics = await _varianceManager.GetMetricsAsync(ct);
        var allRecords = await _varianceManager.GetAllAsync(parsedStatus, ct);

        // Apply Tab Filtering
        if (ActiveTab == "pending")
        {
            Variances = allRecords.Where(v => v.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase)).ToList();
        }
        else if (ActiveTab == "shortages")
        {
            Variances = allRecords.Where(v => v.IsShortage).ToList();
        }
        else if (ActiveTab == "overages")
        {
            Variances = allRecords.Where(v => v.IsOverage).ToList();
        }
        else
        {
            Variances = allRecords;
        }

        ViewData["Title"] = "Cash Variance & Float Audits";
        ViewData["ActivePage"] = "CashVariance";
        ViewData["PageDescription"] = "Reconcile register count discrepancies, manage shift floats, and execute supervisory audit reviews.";

        return Page();
    }

    public async Task<IActionResult> OnGetSearchShiftsAsync([FromQuery] string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);
        var shifts = await _apiClient.GetAsync<List<CashierShiftDto>>("/api/cash/shifts?page=1&pageSize=30", ct) ?? new();
        var query = q?.Trim().ToLowerInvariant();

        var results = shifts
            .Where(s => string.IsNullOrEmpty(query) ||
                        s.CashierShiftId.ToString().ToLowerInvariant().Contains(query) ||
                        s.Status.ToString().ToLowerInvariant().Contains(query) ||
                        (s.Notes?.ToLowerInvariant().Contains(query) ?? false))
            .Select(s => new
            {
                id = s.CashierShiftId.ToString(),
                title = $"Shift #{s.CashierShiftId.ToString()[..8]} ({s.OpenedAtUtc:dd MMM HH:mm})",
                sub = $"Float: {s.OpeningFloat:N0} XAF | Exp. Close: {(s.ExpectedClosingAmount?.ToString("N0") ?? "0")} XAF | {s.Status}",
                badge = s.Status.ToString(),
                expected = s.ExpectedClosingAmount ?? s.OpeningFloat,
                actual = s.ClosingFloat ?? 0
            });

        return new JsonResult(results);
    }

    public async Task<IActionResult> OnGetExportCsvAsync([FromQuery] string? status, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return Unauthorized();

        if (!HasPermission(permissions, PermissionKeys.CashRead) && !HasPermission(permissions, PermissionKeys.ReportsRead))
            return Forbid();

        _apiClient.SetToken(token);

        CashVarianceStatus? parsed = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CashVarianceStatus>(status, true, out var s))
            parsed = s;

        var list = await _varianceManager.GetAllAsync(parsed, ct);
        var metrics = await _varianceManager.GetMetricsAsync(ct);
        var csvBytes = _varianceManager.GenerateCsv(list, metrics);

        return File(csvBytes, "text/csv", $"cash_variances_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv");
    }

    public async Task<IActionResult> OnPostRecordAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        if (!HasPermission(permissions, PermissionKeys.CashWrite))
            return AccessDenied();

        _apiClient.SetToken(token);

        try
        {
            var result = await _varianceManager.RecordAsync(new RecordCashVarianceRequest
            {
                CashierShiftId = RecordShiftId,
                ExpectedAmount = RecordExpected,
                ActualAmount = RecordActual,
                ReasonCode = RecordReasonCode,
                Notes = RecordNotes
            }, ct);

            StatusMessage = result is not null
                ? "Cash count variance recorded successfully."
                : "Error: Could not record cash variance.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Failed to record cash variance: {ex.Message}";
        }

        return RedirectToPage(new { tab = ActiveTab });
    }

    public async Task<IActionResult> OnPostReviewAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        if (!HasPermission(permissions, PermissionKeys.CashWrite))
            return AccessDenied();

        _apiClient.SetToken(token);

        try
        {
            var result = await _varianceManager.ReviewAsync(ReviewVarianceId, new ReviewCashVarianceRequest
            {
                Status = ReviewStatus,
                ReviewNotes = ReviewNotes
            }, ct);

            StatusMessage = result is not null
                ? $"Variance record #{ReviewVarianceId} updated to {ReviewStatus}."
                : "Error: Failed to review variance record.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Review operation failed: {ex.Message}";
        }

        return RedirectToPage(new { tab = ActiveTab });
    }
}

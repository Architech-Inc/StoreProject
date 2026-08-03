using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Cash;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class CashVarianceModel : SecurePageModel
{
    private readonly ICashVarianceService _varianceService;
    private readonly IApiClientService _apiClient;

    public List<CashVarianceDto> Variances { get; private set; } = new();
    public string? FilterStatus { get; private set; }

    // ---- Record Variance ----
    [BindProperty] public Guid RecordShiftId { get; set; }
    [BindProperty] public decimal RecordExpected { get; set; }
    [BindProperty] public decimal RecordActual { get; set; }
    [BindProperty] public string? RecordReasonCode { get; set; }
    [BindProperty] public string? RecordNotes { get; set; }

    // ---- Review Variance ----
    [BindProperty] public int ReviewVarianceId { get; set; }
    [BindProperty] public CashVarianceStatus ReviewStatus { get; set; }
    [BindProperty] public string? ReviewNotes { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public IEnumerable<CashVarianceStatus> ReviewableStatuses { get; } =
        new[] { CashVarianceStatus.Reviewed, CashVarianceStatus.Escalated };

    public CashVarianceModel(ICashVarianceService varianceService, IApiClientService apiClient)
    {
        _varianceService = varianceService;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync([FromQuery] string? status = null)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        FilterStatus = status;

        CashVarianceStatus? parsed = null;
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<CashVarianceStatus>(status, true, out var s))
            parsed = s;

        Variances = await _varianceService.GetAllAsync(parsed);
        ViewData["ActivePage"] = "CashVariance";
        return Page();
    }

    public async Task<IActionResult> OnGetSearchShiftsAsync([FromQuery] string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);
        var shifts = await _apiClient.GetAsync<List<Store.Models.DTOs.Operations.CashierShiftDto>>("/api/cash/shifts?page=1&pageSize=20", ct) ?? new();
        var query = q?.Trim().ToLowerInvariant();
        var results = shifts
            .Where(s => string.IsNullOrEmpty(query) ||
                        s.CashierShiftId.ToString().ToLowerInvariant().Contains(query) ||
                        s.Status.ToString().ToLowerInvariant().Contains(query) ||
                        (s.Notes?.ToLowerInvariant().Contains(query) ?? false))
            .Select(s => new
            {
                id = s.CashierShiftId.ToString(),
                title = $"Shift #{s.CashierShiftId.ToString()[..8]} ({s.OpenedAtUtc:yyyy-MM-dd HH:mm})",
                sub = $"Float: {s.OpeningFloat:N2} | Closed: {(s.ClosedAtUtc.HasValue ? s.ClosedAtUtc.Value.ToString("HH:mm") : "Open")} | {(s.Notes ?? "")}",
                badge = s.Status.ToString()
            });

        return new JsonResult(results);
    }

    public async Task<IActionResult> OnPostRecordAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        try
        {
            await _varianceService.RecordAsync(new RecordCashVarianceRequest
            {
                CashierShiftId = RecordShiftId,
                ExpectedAmount = RecordExpected,
                ActualAmount = RecordActual,
                ReasonCode = RecordReasonCode,
                Notes = RecordNotes
            }, Guid.Empty);

            StatusMessage = "Cash variance recorded successfully.";
        }
        catch
        {
            StatusMessage = "Error: Failed to record cash variance.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReviewAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var result = await _varianceService.ReviewAsync(ReviewVarianceId, Guid.Empty,
            new ReviewCashVarianceRequest
            {
                Status = ReviewStatus,
                ReviewNotes = ReviewNotes
            });

        StatusMessage = result is not null
            ? "Variance review submitted."
            : "Error: Could not review — must be in Pending status.";

        return RedirectToPage();
    }
}

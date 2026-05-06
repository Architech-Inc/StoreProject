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

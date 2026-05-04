using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Discounts;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class DiscountOverridesModel : SecurePageModel
{
    private readonly IDiscountOverrideService _overrideService;
    private readonly IApiClientService _apiClient;

    public List<DiscountOverrideDto> Overrides { get; private set; } = new();
    public string? FilterStatus { get; private set; }

    // Create
    [BindProperty] public Guid? CreateInvoiceId { get; set; }
    [BindProperty] public Guid? CreateItemId { get; set; }
    [BindProperty] public DiscountType CreateOverrideType { get; set; }
    [BindProperty] public decimal CreateOverrideValue { get; set; }
    [BindProperty] public string? CreateJustification { get; set; }

    // Review
    [BindProperty] public int ReviewRequestId { get; set; }
    [BindProperty] public bool ReviewApproved { get; set; }
    [BindProperty] public string? ReviewNotes { get; set; }

    // Cancel
    [BindProperty] public int CancelRequestId { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public IEnumerable<DiscountType> DiscountTypes { get; } = Enum.GetValues<DiscountType>();

    public DiscountOverridesModel(IDiscountOverrideService overrideService, IApiClientService apiClient)
    {
        _overrideService = overrideService;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync([FromQuery] string? status = null)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        FilterStatus = status;
        Overrides = await _overrideService.GetAllAsync(status);
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var req = new CreateDiscountOverrideRequest
        {
            InvoiceId = CreateInvoiceId,
            ItemId = CreateItemId,
            OverrideType = CreateOverrideType,
            OverrideValue = CreateOverrideValue,
            Justification = CreateJustification
        };

        await _overrideService.CreateAsync(req, Guid.Empty); // userId resolved by API via JWT
        StatusMessage = "Override request submitted.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReviewAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var req = new ReviewDiscountOverrideRequest
        {
            Approved = ReviewApproved,
            ReviewNotes = ReviewNotes
        };

        var result = await _overrideService.ReviewAsync(ReviewRequestId, Guid.Empty, req);
        StatusMessage = result is not null
            ? (ReviewApproved ? "Override approved." : "Override rejected.")
            : "Request is no longer pending or was not found.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var ok = await _overrideService.CancelAsync(CancelRequestId, Guid.Empty);
        StatusMessage = ok ? "Override request cancelled." : "Request is no longer pending or was not found.";
        return RedirectToPage();
    }
}

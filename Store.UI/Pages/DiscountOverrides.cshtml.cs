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
    private readonly IInvoiceService _invoiceService;
    private readonly IItemService _itemService;

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

    public DiscountOverridesModel(
        IDiscountOverrideService overrideService,
        IApiClientService apiClient,
        IInvoiceService invoiceService,
        IItemService itemService)
    {
        _overrideService = overrideService;
        _apiClient = apiClient;
        _invoiceService = invoiceService;
        _itemService = itemService;
    }

    public async Task<IActionResult> OnGetSearchInvoicesAsync([FromQuery] string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);
        var result = await _invoiceService.GetAllAsync(new Store.Models.DTOs.Common.PagedRequest { Page = 1, PageSize = 15 }, ct);
        var query = q?.Trim().ToLowerInvariant();
        var items = result.Items
            .Where(inv => string.IsNullOrEmpty(query) ||
                          (inv.CustomerName?.ToLowerInvariant().Contains(query) == true) ||
                          inv.InvoiceId.ToString().ToLowerInvariant().Contains(query))
            .Select(inv => new
            {
                id = inv.InvoiceId.ToString(),
                title = $"Invoice #{inv.InvoiceId.ToString()[..8]} - {inv.TotalAmount:N0} XAF",
                sub = $"Customer: {inv.CustomerName ?? "Walk-in"} | Date: {inv.DateCreated:yyyy-MM-dd HH:mm}",
                badge = inv.IsPaid ? "Paid" : "Pending"
            });

        return new JsonResult(items);
    }

    public async Task<IActionResult> OnGetSearchItemsAsync([FromQuery] string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);
        var result = await _itemService.GetAllAsync(new Store.Models.DTOs.Common.PagedRequest { Page = 1, PageSize = 15, SearchTerm = q?.Trim() }, ct);
        var items = result.Items.Select(i => new
        {
            id = i.ItemId.ToString(),
            title = i.Name,
            sub = $"Barcode: {(string.IsNullOrEmpty(i.Barcode) ? "N/A" : i.Barcode)} | Price: {i.UnitPrice:N2} XAF",
            badge = i.CategoryName ?? "Item"
        });

        return new JsonResult(items);
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

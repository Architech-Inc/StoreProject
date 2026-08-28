using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Discounts;
using Store.Models.DTOs.Operations;
using Store.Models.Enums;
using StoreUI.Services;

namespace StoreUI.Pages;

public class DiscountOverridesModel : SecurePageModel
{
    private readonly IDiscountOverrideManager _overrideManager;
    private readonly IApiClientService _apiClient;

    public DiscountOverrideMetricsDto Metrics { get; private set; } = new();
    public PagedResult<DiscountOverrideDto> OverridesPaged { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? OverrideType { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    // Create
    [BindProperty]
    public CreateDiscountOverrideRequest CreateRequest { get; set; } = new();

    // Review
    [BindProperty]
    public int ReviewRequestId { get; set; }

    [BindProperty]
    public ReviewDiscountOverrideRequest ReviewRequest { get; set; } = new();

    // Cancel
    [BindProperty]
    public int CancelRequestId { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public IEnumerable<DiscountType> DiscountTypes { get; } = Enum.GetValues<DiscountType>();
    public bool CanApprove { get; private set; }

    public DiscountOverridesModel(
        IDiscountOverrideManager overrideManager,
        IApiClientService apiClient)
    {
        _overrideManager = overrideManager;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        if (!HasPermission(permissions, PermissionKeys.PricingRead) &&
            !HasPermission(permissions, PermissionKeys.CashWrite) &&
            !HasPermission(permissions, PermissionKeys.InventoryRead))
        {
            return AccessDenied();
        }

        CanApprove = HasPermission(permissions, PermissionKeys.PricingWrite);
        _apiClient.SetToken(token);

        Metrics = await _overrideManager.GetMetricsAsync(ct);

        var filter = new DiscountOverrideFilterRequest
        {
            Page = PageNumber < 1 ? 1 : PageNumber,
            PageSize = 20,
            SearchTerm = Search,
            Status = Status,
            OverrideType = OverrideType
        };

        OverridesPaged = await _overrideManager.GetOverridesPagedAsync(filter, ct);
        return Page();
    }

    public async Task<IActionResult> OnGetDetailsJsonAsync(int id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var dto = await _overrideManager.GetOverrideByIdAsync(id, ct);
        if (dto is null) return NotFound();

        return new JsonResult(dto);
    }

    public async Task<IActionResult> OnGetSearchInvoicesAsync([FromQuery] string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var invoices = await _overrideManager.SearchInvoicesAsync(q, ct);
        var items = invoices.Select(inv => new
        {
            id = inv.InvoiceId.ToString(),
            title = $"Invoice #{inv.InvoiceId.ToString()[..8]} - {inv.TotalAmount:N0} XAF",
            sub = $"Customer: {inv.CustomerName ?? "Walk-in"} | Total: {inv.TotalAmount:N0} XAF",
            totalAmount = inv.TotalAmount,
            badge = inv.IsPaid ? "Paid" : "Pending"
        });

        return new JsonResult(items);
    }

    public async Task<IActionResult> OnGetSearchItemsAsync([FromQuery] string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var items = await _overrideManager.SearchItemsAsync(q, ct);
        var list = items.Select(i => new
        {
            id = i.ItemId.ToString(),
            title = i.Name,
            sub = $"Barcode: {(string.IsNullOrEmpty(i.Barcode) ? "N/A" : i.Barcode)} | Price: {i.UnitPrice:N0} XAF",
            unitPrice = i.UnitPrice,
            badge = i.CategoryName ?? "General"
        });

        return new JsonResult(list);
    }

    public async Task<IActionResult> OnGetExportCsvAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var filter = new DiscountOverrideFilterRequest
        {
            Page = 1,
            PageSize = 2000,
            SearchTerm = Search,
            Status = Status,
            OverrideType = OverrideType
        };

        var paged = await _overrideManager.GetOverridesPagedAsync(filter, ct);
        var bytes = _overrideManager.ExportCsv(paged.Items);
        var filename = $"discount_overrides_ledger_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        return File(bytes, "text/csv", filename);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        try
        {
            var result = await _overrideManager.CreateOverrideAsync(CreateRequest, Guid.Empty, ct);
            StatusMessage = $"Discount override request #{result.DiscountOverrideRequestId} submitted for manager review.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Failed to submit override request - {ex.Message}";
        }

        return RedirectToPage(new { Search, Status, OverrideType, PageNumber });
    }

    public async Task<IActionResult> OnPostReviewAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        if (!HasPermission(permissions, PermissionKeys.PricingWrite))
        {
            StatusMessage = "Error: You do not have supervisory authority to approve or reject discount overrides.";
            return RedirectToPage(new { Search, Status, OverrideType, PageNumber });
        }

        _apiClient.SetToken(token);

        try
        {
            var result = await _overrideManager.ReviewOverrideAsync(ReviewRequestId, Guid.Empty, ReviewRequest, ct);
            StatusMessage = result is not null
                ? (ReviewRequest.Approved ? $"Override #{ReviewRequestId} approved successfully." : $"Override #{ReviewRequestId} rejected.")
                : "Request is no longer pending or was not found.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Failed to review override request - {ex.Message}";
        }

        return RedirectToPage(new { Search, Status, OverrideType, PageNumber });
    }

    public async Task<IActionResult> OnPostCancelAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        try
        {
            var ok = await _overrideManager.CancelOverrideAsync(CancelRequestId, Guid.Empty, ct);
            StatusMessage = ok ? $"Override request #{CancelRequestId} cancelled." : "Request is no longer pending or was not found.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Failed to cancel override - {ex.Message}";
        }

        return RedirectToPage(new { Search, Status, OverrideType, PageNumber });
    }
}

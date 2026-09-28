using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Discounts;
using Store.Models.DTOs.Operations;
using Store.Models.Enums;
using StoreUI.Services;
using Store.Models.Common;

using Microsoft.Extensions.Logging.Abstractions;
namespace StoreUI.Pages;

public class DiscountsModel : SecurePageModel
{
    private readonly IDiscountManager _discountManager;
    private readonly IApiClientService _apiClient;

    public DiscountMetricsDto Metrics { get; private set; } = new();
    public PagedResult<DiscountDto> DiscountsPaged { get; private set; } = new();

    // UX-05 — secondary affordance gates. Mirrored on the server side
    // in the OnPost handlers below; never trust the UI flag alone.
    public bool CanCreate { get; private set; }
    public bool CanEdit { get; private set; }
    public bool CanDelete { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? TypeFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SegmentFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? ActiveOnly { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty]
    public CreateDiscountRequest CreateRequest { get; set; } = new();

    [BindProperty]
    public int EditDiscountId { get; set; }

    [BindProperty]
    public UpdateDiscountRequest EditRequest { get; set; } = new();

    [BindProperty]
    public int DeleteDiscountId { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public IEnumerable<CustomerSegment> Segments { get; } = Enum.GetValues<CustomerSegment>();
    public IEnumerable<DiscountType> DiscountTypes { get; } = Enum.GetValues<DiscountType>();

    public DiscountsModel(IDiscountManager discountManager, IApiClientService apiClient)
    {
        _discountManager = discountManager;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        if (!HasPermission(permissions, PermissionKeys.PricingRead) &&
            !HasPermission(permissions, PermissionKeys.InventoryRead))
        {
            return AccessDenied();
        }

        // UX-05 — secondary affordance gates. Set from the same
        // permission list so server + UI stay in sync.
        CanCreate = HasPermission(permissions, PermissionKeys.DiscountWrite);
        CanEdit = HasPermission(permissions, PermissionKeys.DiscountWrite);
        CanDelete = HasPermission(permissions, PermissionKeys.DiscountWrite);

        _apiClient.SetToken(token);

        Metrics = await _discountManager.GetMetricsAsync(ct);

        var filter = new DiscountFilterRequest
        {
            Page = PageNumber < 1 ? 1 : PageNumber,
            PageSize = 20,
            SearchTerm = Search,
            DiscountType = TypeFilter,
            TargetSegment = SegmentFilter,
            ActiveOnly = ActiveOnly
        };

        DiscountsPaged = await _discountManager.GetDiscountsPagedAsync(filter, ct);
        return Page();
    }

    public async Task<IActionResult> OnGetDetailsJsonAsync(int id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var discount = await _discountManager.GetDiscountByIdAsync(id, ct);
        if (discount is null) return NotFound();

        return new JsonResult(discount);
    }

    public async Task<IActionResult> OnGetSearchCatalogAsync(string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var items = await _discountManager.SearchCatalogItemsAsync(q, ct);
        var list = items.Select(i => new
        {
            id = i.ItemId.ToString(),
            name = i.Name,
            barcode = i.Barcode ?? "N/A",
            category = i.CategoryName ?? "General",
            unitPrice = i.UnitPrice
        });

        return new JsonResult(list);
    }

    public async Task<IActionResult> OnGetCategoriesAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var categories = await _discountManager.GetCategoriesAsync(ct);
        var list = categories.Select(c => new
        {
            id = c.CategoryId,
            name = c.Name
        });

        return new JsonResult(list);
    }

    public async Task<IActionResult> OnPostSimulateAsync([FromBody] DiscountSimulationRequest request, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var result = await _discountManager.SimulateDiscountAsync(request, ct);
        return new JsonResult(result);
    }

    public async Task<IActionResult> OnGetExportCsvAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var filter = new DiscountFilterRequest
        {
            Page = 1,
            PageSize = 2000,
            SearchTerm = Search,
            DiscountType = TypeFilter,
            TargetSegment = SegmentFilter,
            ActiveOnly = ActiveOnly
        };

        var paged = await _discountManager.GetDiscountsPagedAsync(filter, ct);
        var bytes = _discountManager.ExportCsv(paged.Items);
        var filename = $"discount_rules_catalog_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        return File(bytes, "text/csv", filename);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        // UX-05 — server-side gate (UI hides the button via CanCreate,
        // but never trust the client). DiscountWrite covers create.
        if (!HasPermission(permissions, PermissionKeys.DiscountWrite))
        {
            StatusMessage = "Error: You do not have permission to create discounts.";
            return RedirectToPage(new { Search, TypeFilter, SegmentFilter, ActiveOnly, PageNumber });
        }

        _apiClient.SetToken(token);

        if (string.IsNullOrWhiteSpace(CreateRequest.Name))
        {
            StatusMessage = "Error: Rule name is required.";
            return RedirectToPage(new { Search, TypeFilter, SegmentFilter, ActiveOnly, PageNumber });
        }

        try
        {
            await _discountManager.CreateDiscountAsync(CreateRequest, Guid.Empty, ct);
            StatusMessage = $"Discount rule '{CreateRequest.Name}' created successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Failed to create discount - {SafeErrorMessage.From(ex, NullLogger<DiscountsModel>.Instance, "Discounts operation")}";
        }

        return RedirectToPage(new { Search, TypeFilter, SegmentFilter, ActiveOnly, PageNumber });
    }

    public async Task<IActionResult> OnPostEditAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        // UX-05 — server-side gate. DiscountWrite covers edit.
        if (!HasPermission(permissions, PermissionKeys.DiscountWrite))
        {
            StatusMessage = "Error: You do not have permission to edit discounts.";
            return RedirectToPage(new { Search, TypeFilter, SegmentFilter, ActiveOnly, PageNumber });
        }

        _apiClient.SetToken(token);

        try
        {
            var result = await _discountManager.UpdateDiscountAsync(EditDiscountId, EditRequest, ct);
            StatusMessage = result != null ? "Discount rule updated successfully." : "Error: Discount not found.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Failed to update discount - {SafeErrorMessage.From(ex, NullLogger<DiscountsModel>.Instance, "Discounts operation")}";
        }

        return RedirectToPage(new { Search, TypeFilter, SegmentFilter, ActiveOnly, PageNumber });
    }

    public async Task<IActionResult> OnPostDeleteAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        // UX-05 — server-side gate. DiscountWrite covers delete (deletion
        // is destructive — a stricter admin-only key would be possible
        // later via an AdminDiscounts permission, but for now the write
        // key gates the whole CRUD set).
        if (!HasPermission(permissions, PermissionKeys.DiscountWrite))
        {
            StatusMessage = "Error: You do not have permission to delete discounts.";
            return RedirectToPage(new { Search, TypeFilter, SegmentFilter, ActiveOnly, PageNumber });
        }

        _apiClient.SetToken(token);

        try
        {
            var ok = await _discountManager.DeleteDiscountAsync(DeleteDiscountId, ct);
            StatusMessage = ok ? "Discount rule deleted." : "Error: Discount rule not found.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Failed to delete discount - {SafeErrorMessage.From(ex, NullLogger<DiscountsModel>.Instance, "Discounts operation")}";
        }

        return RedirectToPage(new { Search, TypeFilter, SegmentFilter, ActiveOnly, PageNumber });
    }
}

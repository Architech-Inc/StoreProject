using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Inventory;
using Store.Models.Enums;
using StoreUI.Services;

namespace StoreUI.Pages;

public class WastageModel : SecurePageModel
{
    private readonly IWastageManager _wastageManager;
    private readonly IApiClientService _apiClient;

    public WastageMetricsDto Metrics { get; private set; } = new();
    public PagedResult<WastageEntryDto> WastagePaged { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? WastageTypeFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty]
    public RecordWastageRequest RecordRequest { get; set; } = new();

    [BindProperty]
    public int DeleteEntryId { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public IEnumerable<WastageType> AvailableWastageTypes { get; } = Enum.GetValues<WastageType>();

    public WastageModel(IWastageManager wastageManager, IApiClientService apiClient)
    {
        _wastageManager = wastageManager;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        Metrics = await _wastageManager.GetMetricsAsync(ct);

        var filter = new WastageFilterRequest
        {
            Page = PageNumber < 1 ? 1 : PageNumber,
            PageSize = 20,
            WastageType = string.IsNullOrWhiteSpace(WastageTypeFilter) ? null : WastageTypeFilter,
            SearchTerm = Search
        };

        WastagePaged = await _wastageManager.GetWastagePagedAsync(filter, ct);
        return Page();
    }

    public async Task<IActionResult> OnGetExportCsvAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var filter = new WastageFilterRequest
        {
            Page = 1,
            PageSize = 1000,
            WastageType = string.IsNullOrWhiteSpace(WastageTypeFilter) ? null : WastageTypeFilter,
            SearchTerm = Search
        };

        var paged = await _wastageManager.GetWastagePagedAsync(filter, ct);
        var bytes = _wastageManager.ExportCsv(paged.Items);
        var filename = $"wastage_loss_report_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        return File(bytes, "text/csv", filename);
    }

    public async Task<IActionResult> OnPostRecordAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        if (RecordRequest.ItemId == Guid.Empty || RecordRequest.Quantity <= 0)
        {
            StatusMessage = "Error: Please select a valid item and specify a quantity greater than zero.";
            return RedirectToPage(new { Search, WastageTypeFilter, PageNumber });
        }

        try
        {
            await _wastageManager.RecordWastageAsync(RecordRequest, Guid.Empty, ct);
            StatusMessage = "Wastage write-off entry recorded successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Failed to record wastage entry - {ex.Message}";
        }

        return RedirectToPage(new { Search, WastageTypeFilter, PageNumber });
    }

    public async Task<IActionResult> OnPostDeleteAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        try
        {
            var ok = await _wastageManager.DeleteWastageAsync(DeleteEntryId, ct);
            StatusMessage = ok ? "Wastage entry removed successfully." : "Error: Wastage entry not found.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Failed to delete wastage entry - {ex.Message}";
        }

        return RedirectToPage(new { Search, WastageTypeFilter, PageNumber });
    }

    public async Task<IActionResult> OnGetSearchCatalogAsync(string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var items = await _wastageManager.SearchCatalogItemsAsync(q, ct);
        var result = items.Select(i => new
        {
            id = i.ItemId.ToString(),
            name = i.Name,
            barcode = i.Barcode ?? "N/A",
            category = i.CategoryName ?? "General",
            inStock = i.InStock,
            costPrice = i.CostPrice ?? 0,
            unitPrice = i.UnitPrice
        });

        return new JsonResult(result);
    }
}

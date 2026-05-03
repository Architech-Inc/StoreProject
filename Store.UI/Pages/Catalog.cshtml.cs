using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Items;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class CatalogModel : SecurePageModel
{
    private readonly IItemService _itemService;
    private readonly IApiClientService _apiClient;

    public IReadOnlyList<ItemDto> Items { get; private set; } = Array.Empty<ItemDto>();
    public IReadOnlyList<Category> Categories { get; private set; } = Array.Empty<Category>();
    public IReadOnlyList<Unit> Units { get; private set; } = Array.Empty<Unit>();
    public int TotalItems { get; private set; }
    public int PageNumber { get; private set; } = 1;
    public int PageSize { get; private set; } = 25;
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    [BindProperty] public Guid? EditItemId { get; set; }
    [BindProperty] public string ItemName { get; set; } = string.Empty;
    [BindProperty] public string? ItemDescription { get; set; }
    [BindProperty] public decimal ItemUnitPrice { get; set; }
    [BindProperty] public decimal? ItemCostPrice { get; set; }
    [BindProperty] public int ItemInStock { get; set; }
    [BindProperty] public int? ItemReorderLevel { get; set; }
    [BindProperty] public string? ItemBarcode { get; set; }
    [BindProperty] public int? ItemCategoryId { get; set; }
    [BindProperty] public int? ItemUnitId { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public CatalogModel(IItemService itemService, IApiClientService apiClient)
    {
        _itemService = itemService;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(int page = 1, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        PageNumber = Math.Max(1, page);

        var itemsTask = _itemService.GetAllAsync(new PagedRequest { Page = PageNumber, PageSize = PageSize }, ct);
        var catsTask  = _apiClient.GetAsync<List<Category>>("/api/categories", ct);
        var unitsTask = _apiClient.GetAsync<List<Unit>>("/api/units", ct);

        var result = await itemsTask;
        Items      = result.Items.ToList();
        TotalItems = result.TotalCount;
        Categories = (await catsTask)  ?? new();
        Units      = (await unitsTask) ?? new();

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        if (EditItemId.HasValue && EditItemId.Value != Guid.Empty)
        {
            var req = new UpdateItemRequest
            {
                Name         = ItemName,
                Description  = ItemDescription,
                UnitPrice    = ItemUnitPrice,
                CostPrice    = ItemCostPrice,
                ReorderLevel = ItemReorderLevel,
                Barcode      = ItemBarcode,
                CategoryId   = ItemCategoryId,
                UnitId       = ItemUnitId
            };
            await _itemService.UpdateAsync(EditItemId.Value, req, ct);
            StatusMessage = "Item updated.";
        }
        else
        {
            var req = new CreateItemRequest
            {
                Name         = ItemName,
                Description  = ItemDescription,
                UnitPrice    = ItemUnitPrice,
                CostPrice    = ItemCostPrice,
                InStock      = ItemInStock,
                ReorderLevel = ItemReorderLevel,
                Barcode      = ItemBarcode,
                CategoryId   = ItemCategoryId,
                UnitId       = ItemUnitId,
                Type         = ItemType.Product
            };
            await _itemService.CreateAsync(req, ct);
            StatusMessage = "Item created.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid itemId, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        await _itemService.UpdateAsync(itemId, new UpdateItemRequest { IsActive = false }, ct);
        StatusMessage = "Item deactivated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid itemId, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        await _itemService.DeleteAsync(itemId, ct);
        StatusMessage = "Item deleted.";
        return RedirectToPage();
    }
}

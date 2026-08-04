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
    private readonly IFileService _fileService;

    public IReadOnlyList<ItemDto> Items { get; private set; } = Array.Empty<ItemDto>();
    public IReadOnlyList<Category> Categories { get; private set; } = Array.Empty<Category>();
    public IReadOnlyList<Unit> Units { get; private set; } = Array.Empty<Unit>();
    public int TotalItems { get; private set; }
    public int PageNumber { get; private set; } = 1;
    public int PageSize { get; private set; } = 25;
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    // KPI Metrics
    public int TotalSkus { get; private set; }
    public int LowStockCount { get; private set; }
    public int OutOfStockCount { get; private set; }
    public decimal TotalInventoryCostValue { get; private set; }
    public decimal TotalInventoryRetailValue { get; private set; }

    // Search and Filter parameters
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int? CategoryId { get; set; }
    [BindProperty(SupportsGet = true)] public string? StockStatus { get; set; } = "all";
    [BindProperty(SupportsGet = true)] public string? SortBy { get; set; } = "name";
    [BindProperty(SupportsGet = true)] public string ViewMode { get; set; } = "table";

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
    [BindProperty] public ItemType ItemType { get; set; } = ItemType.Product;
    [BindProperty] public IFormFile? ImageUpload { get; set; }

    [BindProperty] public int? CropX { get; set; }
    [BindProperty] public int? CropY { get; set; }
    [BindProperty] public int? CropW { get; set; }
    [BindProperty] public int? CropH { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public CatalogModel(IItemService itemService, IApiClientService apiClient, IFileService fileService)
    {
        _itemService = itemService;
        _apiClient = apiClient;
        _fileService = fileService;
    }

    public async Task<IActionResult> OnGetAsync(int page = 1, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        PageNumber = Math.Max(1, page);

        var pagedReq = new PagedRequest
        {
            Page = PageNumber,
            PageSize = PageSize,
            IncludeInactive = true,
            SearchTerm = Search,
            CategoryId = CategoryId,
            StockStatus = StockStatus,
            SortBy = SortBy
        };

        var itemsTask = _itemService.GetAllAsync(pagedReq, ct);
        var allItemsTask = _itemService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 1000, IncludeInactive = true }, ct);
        var catsTask  = _apiClient.GetAsync<List<Category>>("/api/categories", ct);
        var unitsTask = _apiClient.GetAsync<List<Unit>>("/api/units", ct);

        var result = await itemsTask;
        Items      = result.Items.ToList();
        TotalItems = result.TotalCount;
        Categories = (await catsTask)  ?? new();
        Units      = (await unitsTask) ?? new();

        var allItemsResult = await allItemsTask;
        var allList = allItemsResult?.Items ?? Enumerable.Empty<ItemDto>();
        TotalSkus = allList.Count(i => i.IsActive);
        LowStockCount = allList.Count(i => i.IsActive && i.InStock > 0 && i.ReorderLevel.HasValue && i.InStock <= i.ReorderLevel.Value);
        OutOfStockCount = allList.Count(i => i.IsActive && i.InStock <= 0);
        TotalInventoryCostValue = allList.Where(i => i.IsActive).Sum(i => (i.CostPrice ?? i.UnitPrice) * i.InStock);
        TotalInventoryRetailValue = allList.Where(i => i.IsActive).Sum(i => i.UnitPrice * i.InStock);

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        string? thumbUrl = null;
        string? fullUrl = null;
        if (ImageUpload != null && ImageUpload.Length > 0)
        {
            if (EditItemId.HasValue && EditItemId.Value != Guid.Empty)
            {
                var existingItem = await _itemService.GetByIdAsync(EditItemId.Value, ct);
                if (existingItem != null)
                {
                    if (!string.IsNullOrWhiteSpace(existingItem.ThumbnailUrl))
                        await _fileService.DeleteFileAsync(existingItem.ThumbnailUrl, ct);
                    if (!string.IsNullOrWhiteSpace(existingItem.FullImageUrl))
                        await _fileService.DeleteFileAsync(existingItem.FullImageUrl, ct);
                }
            }
            using var stream = ImageUpload.OpenReadStream();
            var uploadResult = await _fileService.UploadFileAsync(stream, ImageUpload.FileName, ImageUpload.ContentType, "items", CropX, CropY, CropW, CropH, ct);
            thumbUrl = uploadResult.ThumbnailUrl;
            fullUrl = uploadResult.FullImageUrl;
        }

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
                UnitId       = ItemUnitId,
                Type         = ItemType,
                ThumbnailUrl = thumbUrl,
                FullImageUrl = fullUrl
            };
            await _itemService.UpdateAsync(EditItemId.Value, req, ct);
            StatusMessage = "Item updated successfully.";
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
                Type         = ItemType,
                ThumbnailUrl = thumbUrl,
                FullImageUrl = fullUrl
            };
            await _itemService.CreateAsync(req, ct);
            StatusMessage = "Item created successfully.";
        }

        return RedirectToPage("/Catalog", new { page = PageNumber, search = Search, categoryId = CategoryId, stockStatus = StockStatus, sortBy = SortBy, viewMode = ViewMode });
    }

    public async Task<IActionResult> OnPostAdjustStockAsync(Guid itemId, int adjustmentQuantity, string? reason, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        var success = await _itemService.AdjustStockAsync(itemId, new AdjustStockRequest
        {
            ItemId = itemId,
            AdjustmentQuantity = adjustmentQuantity,
            Reason = reason ?? "Manual catalog adjustment"
        }, ct);

        StatusMessage = success ? $"Stock adjusted by {adjustmentQuantity:+0;-0;0} units." : "Failed to adjust stock.";
        return RedirectToPage("/Catalog", new { page = PageNumber, search = Search, categoryId = CategoryId, stockStatus = StockStatus, sortBy = SortBy, viewMode = ViewMode });
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(Guid itemId, bool makeActive, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        await _itemService.UpdateAsync(itemId, new UpdateItemRequest { IsActive = makeActive }, ct);
        StatusMessage = makeActive ? "Item activated." : "Item deactivated.";
        return RedirectToPage("/Catalog", new { page = PageNumber, search = Search, categoryId = CategoryId, stockStatus = StockStatus, sortBy = SortBy, viewMode = ViewMode });
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid itemId, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        await _itemService.UpdateAsync(itemId, new UpdateItemRequest { IsActive = false }, ct);
        StatusMessage = "Item deactivated.";
        return RedirectToPage("/Catalog", new { page = PageNumber, search = Search, categoryId = CategoryId, stockStatus = StockStatus, sortBy = SortBy, viewMode = ViewMode });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid itemId, CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        await _itemService.DeleteAsync(itemId, ct);
        StatusMessage = "Item deleted.";
        return RedirectToPage("/Catalog", new { page = PageNumber, search = Search, categoryId = CategoryId, stockStatus = StockStatus, sortBy = SortBy, viewMode = ViewMode });
    }
}

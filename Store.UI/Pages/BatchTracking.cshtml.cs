using System.Text;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Inventory;
using Store.Models.DTOs.Items;
using Store.Models.DTOs.Operations;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class BatchTrackingModel : SecurePageModel
{
    private readonly IBatchManager _batchManager;
    private readonly IItemService _itemService;
    private readonly IApiClientService _apiClient;

    public BatchMetricsDto Metrics { get; private set; } = new();
    public List<BatchDto> Batches { get; private set; } = new();
    public List<BatchDto> ExpiringBatches { get; private set; } = new();
    public IReadOnlyList<ItemDto> QuickItems { get; private set; } = Array.Empty<ItemDto>();
    public int TotalBatches { get; private set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalBatches / PageSize);

    // Filters
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? ExpiryStatus { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? ItemId { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;

    // Create Form
    [BindProperty] public Guid CreateItemId { get; set; }
    [BindProperty] public string CreateBatchNumber { get; set; } = string.Empty;
    [BindProperty] public int CreateQuantity { get; set; } = 1;
    [BindProperty] public decimal CreateCostPrice { get; set; }
    [BindProperty] public DateTime CreateReceivedDate { get; set; } = DateTime.Today;
    [BindProperty] public DateTime? CreateExpiryDate { get; set; }
    [BindProperty] public string? CreateNotes { get; set; }

    // Edit Form
    [BindProperty] public Guid EditBatchId { get; set; }
    [BindProperty] public string? EditBatchNumber { get; set; }
    [BindProperty] public int? EditQuantity { get; set; }
    [BindProperty] public decimal? EditCostPrice { get; set; }
    [BindProperty] public DateTime? EditExpiryDate { get; set; }
    [BindProperty] public string? EditNotes { get; set; }

    // Delete Form
    [BindProperty] public Guid DeleteBatchId { get; set; }

    // Write-Off to Wastage Form
    [BindProperty] public Guid WriteOffBatchId { get; set; }
    [BindProperty] public Guid WriteOffItemId { get; set; }
    [BindProperty] public int WriteOffQuantity { get; set; } = 1;
    [BindProperty] public WastageType WriteOffType { get; set; } = WastageType.Expiry;
    [BindProperty] public string WriteOffReason { get; set; } = "Batch expired / damaged";
    [BindProperty] public string? WriteOffNotes { get; set; }

    public bool CanRead { get; private set; }
    public bool CanWrite { get; private set; }

    [TempData] public string? StatusMessage { get; set; }

    public BatchTrackingModel(
        IBatchManager batchManager,
        IItemService itemService,
        IApiClientService apiClient)
    {
        _batchManager = batchManager;
        _itemService = itemService;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(int page = 1, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        _apiClient.SetToken(token);
        CanRead = HasPermission(permissions, PermissionKeys.InventoryRead);
        CanWrite = HasPermission(permissions, PermissionKeys.InventoryWrite);

        if (!CanRead)
            return AccessDenied();

        PageNumber = Math.Max(1, page);

        var metricsTask = _batchManager.GetMetricsAsync(ct);
        var expiringTask = _batchManager.GetExpiringAsync(30, ct);
        var batchesTask = _batchManager.GetBatchesPagedAsync(new BatchFilterRequest
        {
            Page = PageNumber,
            PageSize = PageSize,
            ItemId = ItemId,
            ExpiryStatus = ExpiryStatus,
            SearchTerm = Search
        }, ct);
        var itemsTask = _itemService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 150, IncludeInactive = false }, ct);

        await Task.WhenAll(metricsTask, expiringTask, batchesTask, itemsTask);

        Metrics = await metricsTask ?? new BatchMetricsDto();
        ExpiringBatches = await expiringTask ?? new List<BatchDto>();
        var pagedResult = await batchesTask ?? new PagedResult<BatchDto>();
        Batches = pagedResult.Items.ToList();
        TotalBatches = pagedResult.TotalCount;

        var itemsResult = await itemsTask;
        QuickItems = itemsResult?.Items.OrderBy(i => i.Name).ToList() ?? new List<ItemDto>();

        return Page();
    }

    public async Task<IActionResult> OnGetSearchItemsAsync([FromQuery] string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);
        var result = await _itemService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 20, SearchTerm = q?.Trim() }, ct);
        var items = result.Items.Select(i => new
        {
            id = i.ItemId.ToString(),
            title = i.Name,
            sub = $"Barcode: {(string.IsNullOrEmpty(i.Barcode) ? "N/A" : i.Barcode)} | Stock: {i.InStock} | Cost: {i.CostPrice:N2} XAF",
            badge = i.CategoryName ?? "Item",
            cost = i.CostPrice ?? 0,
            stock = i.InStock
        });

        return new JsonResult(items);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct = default)
    {
        return await ExecuteWriteOperationAsync(async () =>
        {
            var req = new CreateBatchRequest
            {
                ItemId = CreateItemId,
                BatchNumber = CreateBatchNumber.Trim(),
                Quantity = CreateQuantity,
                CostPrice = CreateCostPrice,
                ReceivedDate = CreateReceivedDate,
                ExpiryDate = CreateExpiryDate,
                Notes = string.IsNullOrWhiteSpace(CreateNotes) ? null : CreateNotes.Trim()
            };

            await _batchManager.CreateAsync(req, ct);
            StatusMessage = $"Batch '{req.BatchNumber}' recorded successfully.";
        });
    }

    public async Task<IActionResult> OnPostEditAsync(CancellationToken ct = default)
    {
        return await ExecuteWriteOperationAsync(async () =>
        {
            var req = new UpdateBatchRequest
            {
                BatchNumber = EditBatchNumber?.Trim(),
                Quantity = EditQuantity,
                CostPrice = EditCostPrice,
                ExpiryDate = EditExpiryDate,
                Notes = EditNotes?.Trim()
            };

            await _batchManager.UpdateAsync(EditBatchId, req, ct);
            StatusMessage = "Batch details updated.";
        });
    }

    public async Task<IActionResult> OnPostDeleteAsync(CancellationToken ct = default)
    {
        return await ExecuteWriteOperationAsync(async () =>
        {
            await _batchManager.DeleteAsync(DeleteBatchId, ct);
            StatusMessage = "Batch record removed.";
        });
    }

    public async Task<IActionResult> OnPostWriteOffAsync(CancellationToken ct = default)
    {
        return await ExecuteWriteOperationAsync(async () =>
        {
            var req = new WriteOffBatchRequest
            {
                BatchId = WriteOffBatchId,
                ItemId = WriteOffItemId,
                Quantity = WriteOffQuantity,
                WastageType = WriteOffType,
                Reason = WriteOffReason?.Trim() ?? "Batch expired / damaged",
                Notes = WriteOffNotes?.Trim()
            };

            var ok = await _batchManager.WriteOffAsync(req, ct);
            StatusMessage = ok ? $"Successfully wrote off {req.Quantity} units to Wastage Log." : "Error: Failed to write off batch stock.";
        });
    }

    public async Task<IActionResult> OnGetExportCsvAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        _apiClient.SetToken(token);
        if (!HasPermission(permissions, PermissionKeys.InventoryRead))
            return AccessDenied();

        var result = await _batchManager.GetBatchesPagedAsync(new BatchFilterRequest
        {
            Page = 1,
            PageSize = 5000,
            ItemId = ItemId,
            ExpiryStatus = ExpiryStatus,
            SearchTerm = Search
        }, ct);

        var sb = new StringBuilder();
        sb.AppendLine("Batch Number,Product Name,Category,Barcode,Quantity,Unit Cost (XAF),Total Valuation (XAF),Received Date,Expiry Date,Days Until Expiry,Expiry Status,Notes");

        foreach (var b in result.Items)
        {
            sb.AppendLine($"\"{EscapeCsv(b.BatchNumber)}\",\"{EscapeCsv(b.ItemName)}\",\"{EscapeCsv(b.CategoryName)}\",\"{EscapeCsv(b.ItemCode)}\",\"{b.Quantity}\",\"{b.CostPrice:F2}\",\"{b.TotalValuation:F2}\",\"{b.ReceivedDate:yyyy-MM-dd}\",\"{(b.ExpiryDate.HasValue ? b.ExpiryDate.Value.ToString("yyyy-MM-dd") : "")}\",\"{ (b.ExpiryDate.HasValue ? b.DaysUntilExpiry.ToString() : "N/A") }\",\"{b.ExpiryStatus}\",\"{EscapeCsv(b.Notes)}\"");
        }

        var fileName = $"batch_tracking_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName);
    }

    private async Task<IActionResult> ExecuteWriteOperationAsync(Func<Task> operation)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        _apiClient.SetToken(token);
        if (!HasPermission(permissions, PermissionKeys.InventoryWrite))
            return AccessDenied();

        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }

        return RedirectToPage();
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("\"", "\"\"");
    }
}

using System.Text;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Items;
using Store.Models.DTOs.Operations;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class InventoryOpsModel : SecurePageModel
{
    private readonly IInventoryOpsManager _opsManager;
    private readonly IItemService _itemService;
    private readonly IApiClientService _apiClient;

    public InventoryMetricsDto Metrics { get; private set; } = new();
    public IReadOnlyList<StockMovementDto> Movements { get; private set; } = Array.Empty<StockMovementDto>();
    public IReadOnlyList<ReorderSuggestionDto> ReorderSuggestions { get; private set; } = Array.Empty<ReorderSuggestionDto>();
    public IReadOnlyList<ItemDto> QuickItems { get; private set; } = Array.Empty<ItemDto>();
    public int TotalMovements { get; private set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalMovements / PageSize);

    // Query & Filtering
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public StockMovementType? MovementType { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? ItemId { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;

    // ---- Goods Receipt (GRN) Form ----
    [BindProperty] public Guid ReceiveItemId { get; set; }
    [BindProperty] public int ReceiveQuantity { get; set; } = 1;
    [BindProperty] public decimal? ReceiveUnitCost { get; set; }
    [BindProperty] public string? ReceiveReference { get; set; }
    [BindProperty] public string? ReceiveNotes { get; set; }

    // ---- Stock Return Form ----
    [BindProperty] public Guid ReturnItemId { get; set; }
    [BindProperty] public int ReturnQuantity { get; set; } = 1;
    [BindProperty] public string ReturnReason { get; set; } = "Customer return";
    [BindProperty] public Guid? ReturnInvoiceId { get; set; }

    // ---- Quick Stock Adjustment Form ----
    [BindProperty] public Guid AdjustItemId { get; set; }
    [BindProperty] public int QuantityDelta { get; set; }
    [BindProperty] public string AdjustmentReason { get; set; } = "Physical Count Correction";

    public bool CanRead { get; private set; }
    public bool CanWrite { get; private set; }

    [TempData] public string? StatusMessage { get; set; }

    public InventoryOpsModel(
        IInventoryOpsManager opsManager,
        IItemService itemService,
        IApiClientService apiClient)
    {
        _opsManager = opsManager;
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

        var metricsTask = _opsManager.GetMetricsAsync(ct);
        var reordersTask = _opsManager.GetReorderSuggestionsAsync(ct);
        var movementsTask = _opsManager.GetMovementsAsync(new StockMovementFilterRequest
        {
            Page = PageNumber,
            PageSize = PageSize,
            MovementType = MovementType,
            SearchTerm = Search,
            FromDate = FromDate,
            ToDate = ToDate,
            ItemId = ItemId
        }, ct);
        var itemsTask = _itemService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 150, IncludeInactive = false }, ct);

        await Task.WhenAll(metricsTask, reordersTask, movementsTask, itemsTask);

        Metrics = await metricsTask ?? new InventoryMetricsDto();
        ReorderSuggestions = await reordersTask ?? new List<ReorderSuggestionDto>();
        var movementsResult = await movementsTask ?? new PagedResult<StockMovementDto>();
        Movements = movementsResult.Items.ToList();
        TotalMovements = movementsResult.TotalCount;

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

    public async Task<IActionResult> OnPostReceiveAsync(CancellationToken ct = default)
    {
        return await ExecuteWriteOperationAsync(async () =>
        {
            var req = new GoodsReceiptRequest
            {
                ReferenceCode = ReceiveReference?.Trim(),
                Notes = ReceiveNotes?.Trim(),
                Lines = new List<GoodsReceiptLineRequest>
                {
                    new GoodsReceiptLineRequest
                    {
                        ItemId = ReceiveItemId,
                        Quantity = ReceiveQuantity,
                        UnitCost = ReceiveUnitCost
                    }
                }
            };

            var result = await _opsManager.ReceiveGoodsAsync(req, ct);
            StatusMessage = result.Success ? result.Message : $"Error: {result.Message}";
        });
    }

    public async Task<IActionResult> OnPostReturnAsync(CancellationToken ct = default)
    {
        return await ExecuteWriteOperationAsync(async () =>
        {
            var req = new StockReturnRequest
            {
                ItemId = ReturnItemId,
                Quantity = ReturnQuantity,
                Reason = ReturnReason?.Trim() ?? "Customer return",
                InvoiceId = ReturnInvoiceId
            };

            var result = await _opsManager.ProcessReturnAsync(req, ct);
            StatusMessage = result.Success ? result.Message : $"Error: {result.Message}";
        });
    }

    public async Task<IActionResult> OnPostAdjustAsync(CancellationToken ct = default)
    {
        return await ExecuteWriteOperationAsync(async () =>
        {
            var req = new StockAdjustmentAuditRequest
            {
                ItemId = AdjustItemId,
                QuantityDelta = QuantityDelta,
                Reason = AdjustmentReason?.Trim() ?? "Physical Count Correction"
            };

            var result = await _opsManager.AdjustStockAsync(req, ct);
            StatusMessage = result.Success ? result.Message : $"Error: {result.Message}";
        });
    }

    public async Task<IActionResult> OnGetExportCsvAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        _apiClient.SetToken(token);
        if (!HasPermission(permissions, PermissionKeys.InventoryRead))
            return AccessDenied();

        var result = await _opsManager.GetMovementsAsync(new StockMovementFilterRequest
        {
            Page = 1,
            PageSize = 5000,
            MovementType = MovementType,
            SearchTerm = Search,
            FromDate = FromDate,
            ToDate = ToDate,
            ItemId = ItemId
        }, ct);

        var sb = new StringBuilder();
        sb.AppendLine("Movement ID,Date (UTC),Item Name,Movement Type,Quantity Delta,Stock Before,Stock After,Reason,Reference Code,Performed By");

        foreach (var m in result.Items)
        {
            sb.AppendLine($"\"{m.StockMovementId}\",\"{m.DateCreated:yyyy-MM-dd HH:mm:ss}\",\"{EscapeCsv(m.ItemName)}\",\"{m.MovementType}\",\"{m.QuantityDelta}\",\"{m.StockBefore}\",\"{m.StockAfter}\",\"{EscapeCsv(m.Reason)}\",\"{EscapeCsv(m.ReferenceCode)}\",\"{EscapeCsv(m.PerformedByUserName)}\"");
        }

        var fileName = $"inventory_movements_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
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

using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class InventoryOpsModel : SecurePageModel
{
    private readonly IApiClientService _apiClient;
    private readonly IItemService _itemService;

    public IReadOnlyList<StockMovementDto> Movements { get; private set; } = Array.Empty<StockMovementDto>();
    public IReadOnlyList<ReorderSuggestionDto> ReorderSuggestions { get; private set; } = Array.Empty<ReorderSuggestionDto>();
    public IReadOnlyList<Store.Models.DTOs.Items.ItemDto> Items { get; private set; } = Array.Empty<Store.Models.DTOs.Items.ItemDto>();

    [BindProperty] public Guid ReceiveItemId { get; set; }
    [BindProperty] public int ReceiveQuantity { get; set; } = 1;
    [BindProperty] public decimal? ReceiveUnitCost { get; set; }
    [BindProperty] public string? ReceiveReference { get; set; }

    [BindProperty] public Guid ReturnItemId { get; set; }
    [BindProperty] public int ReturnQuantity { get; set; } = 1;
    [BindProperty] public string ReturnReason { get; set; } = "Customer return";

    [BindProperty] public Guid AdjustItemId { get; set; }
    [BindProperty] public int QuantityDelta { get; set; }
    [BindProperty] public string AdjustmentReason { get; set; } = string.Empty;

    public bool CanRead { get; private set; }
    public bool CanWrite { get; private set; }

    [TempData] public string? StatusMessage { get; set; }

    public InventoryOpsModel(IApiClientService apiClient, IItemService itemService)
    {
        _apiClient = apiClient;
        _itemService = itemService;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
        {
            return GoToLogin();
        }

        _apiClient.SetToken(token);
        CanRead = HasPermission(permissions, PermissionKeys.InventoryRead);
        CanWrite = HasPermission(permissions, PermissionKeys.InventoryWrite);

        if (!CanRead)
        {
            return Forbid();
        }

        await LoadAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostReceiveAsync(CancellationToken ct)
    {
        return await HandleWriteAsync(async () =>
        {
            var req = new GoodsReceiptRequest
            {
                ReferenceCode = ReceiveReference,
                Lines =
                [
                    new GoodsReceiptLineRequest
                    {
                        ItemId = ReceiveItemId,
                        Quantity = ReceiveQuantity,
                        UnitCost = ReceiveUnitCost
                    }
                ]
            };

            var result = await _apiClient.PostAsync<InventoryOperationResultDto>("/api/inventory/receive", req, ct);
            StatusMessage = result?.Message ?? "Receive request submitted.";
        });
    }

    public async Task<IActionResult> OnPostReturnAsync(CancellationToken ct)
    {
        return await HandleWriteAsync(async () =>
        {
            var req = new StockReturnRequest
            {
                ItemId = ReturnItemId,
                Quantity = ReturnQuantity,
                Reason = ReturnReason
            };

            var result = await _apiClient.PostAsync<InventoryOperationResultDto>("/api/inventory/return", req, ct);
            StatusMessage = result?.Message ?? "Return request submitted.";
        });
    }

    public async Task<IActionResult> OnPostAdjustAsync(CancellationToken ct)
    {
        return await HandleWriteAsync(async () =>
        {
            var req = new StockAdjustmentAuditRequest
            {
                ItemId = AdjustItemId,
                QuantityDelta = QuantityDelta,
                Reason = AdjustmentReason
            };

            var result = await _apiClient.PostAsync<InventoryOperationResultDto>("/api/inventory/adjust", req, ct);
            StatusMessage = result?.Message ?? "Adjustment request submitted.";
        });
    }

    private async Task<IActionResult> HandleWriteAsync(Func<Task> operation)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
        {
            return GoToLogin();
        }

        _apiClient.SetToken(token);
        if (!HasPermission(permissions, PermissionKeys.InventoryWrite))
        {
            return Forbid();
        }

        await operation();
        return RedirectToPage();
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        var itemResult = await _itemService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 200 }, ct);
        Items = itemResult.Items.OrderBy(x => x.Name).ToList();

        Movements = await _apiClient.GetAsync<List<StockMovementDto>>("/api/inventory/movements?page=1&pageSize=25", ct)
            ?? new List<StockMovementDto>();

        ReorderSuggestions = await _apiClient.GetAsync<List<ReorderSuggestionDto>>("/api/inventory/reorder", ct)
            ?? new List<ReorderSuggestionDto>();
    }
}

using System.Text;
using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;
using Store.Models.DTOs.Transfers;
using Store.Models.DTOs.Items;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class StockTransfersModel : SecurePageModel
{
    private readonly IStockTransferManager _transferManager;
    private readonly IItemService _itemService;
    private readonly IApiClientService _apiClient;

    public TransferMetricsDto Metrics { get; private set; } = new();
    public List<StockTransferDto> Transfers { get; private set; } = new();
    public IReadOnlyList<BranchDto> QuickBranches { get; private set; } = Array.Empty<BranchDto>();
    public IReadOnlyList<ItemDto> QuickItems { get; private set; } = Array.Empty<ItemDto>();
    public int TotalTransfers { get; private set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalTransfers / PageSize);

    // Filters
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public int? BranchId { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;

    // Create Form
    [BindProperty] public int CreateFromBranchId { get; set; }
    [BindProperty] public int CreateToBranchId { get; set; }
    [BindProperty] public string? CreateNotes { get; set; }
    [BindProperty] public List<TransferItemLine> TransferItems { get; set; } = new();

    // Shared Action Field
    [BindProperty] public int ActionTransferId { get; set; }

    // Approve Form
    [BindProperty] public string? ApproveNotes { get; set; }

    // Reject Form
    [BindProperty] public string? RejectReason { get; set; }

    // Dispatch Form
    [BindProperty] public List<DispatchItemInput> DispatchItems { get; set; } = new();
    [BindProperty] public string? DispatchNotes { get; set; }

    // Receive Form
    [BindProperty] public List<ReceiveItemInput> ReceiveItems { get; set; } = new();
    [BindProperty] public string? ReceiveNotes { get; set; }

    // Cancel Form
    [BindProperty] public string? CancelReason { get; set; }

    public bool CanRead { get; private set; }
    public bool CanWrite { get; private set; }
    public bool CanApprove { get; private set; }

    [TempData] public string? StatusMessage { get; set; }

    public StockTransfersModel(
        IStockTransferManager transferManager,
        IItemService itemService,
        IApiClientService apiClient)
    {
        _transferManager = transferManager;
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
        CanApprove = HasPermission(permissions, PermissionKeys.AdminBranches) || CanWrite;

        if (!CanRead)
            return AccessDenied();

        PageNumber = Math.Max(1, page);

        var metricsTask = _transferManager.GetMetricsAsync(ct);
        var transfersTask = _transferManager.GetTransfersPagedAsync(new TransferFilterRequest
        {
            Page = PageNumber,
            PageSize = PageSize,
            BranchId = BranchId,
            Status = Status,
            SearchTerm = Search
        }, ct);
        var branchesTask = _apiClient.GetAsync<List<BranchDto>>("api/admin/branches", ct);
        var itemsTask = _itemService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 200, IncludeInactive = false }, ct);

        await Task.WhenAll(metricsTask, transfersTask, branchesTask, itemsTask);

        Metrics = await metricsTask ?? new TransferMetricsDto();
        var pagedResult = await transfersTask ?? new PagedResult<StockTransferDto>();
        Transfers = pagedResult.Items.ToList();
        TotalTransfers = pagedResult.TotalCount;

        var branchList = await branchesTask;
        QuickBranches = branchList?.Where(b => b.IsActive).OrderBy(b => b.Name).ToList() ?? new List<BranchDto>();

        var itemList = await itemsTask;
        QuickItems = itemList?.Items.OrderBy(i => i.Name).ToList() ?? new List<ItemDto>();

        return Page();
    }

    public async Task<IActionResult> OnGetSearchItemsAsync([FromQuery] string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);
        var result = await _itemService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 25, SearchTerm = q?.Trim() }, ct);
        var items = result.Items.Select(i => new
        {
            id = i.ItemId.ToString(),
            title = i.Name,
            sub = $"Barcode: {(string.IsNullOrEmpty(i.Barcode) ? "N/A" : i.Barcode)} | In Stock: {i.InStock} | Cost: {(i.CostPrice ?? 0):N2} XAF",
            badge = i.CategoryName ?? "Item",
            cost = i.CostPrice ?? 0,
            stock = i.InStock
        });

        return new JsonResult(items);
    }

    public async Task<IActionResult> OnGetSearchBranchesAsync([FromQuery] string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);
        var branches = await _apiClient.GetAsync<List<BranchDto>>("api/admin/branches", ct) ?? new();
        var search = q?.Trim() ?? string.Empty;
        var filtered = branches
            .Where(b => b.IsActive && (string.IsNullOrEmpty(search) ||
                        b.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        b.Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        b.BranchId.ToString() == search))
            .Take(15)
            .Select(b => new
            {
                id = b.BranchId.ToString(),
                title = b.Name,
                sub = $"Code: {b.Code} | #{b.BranchId}",
                badge = "Branch"
            });

        return new JsonResult(filtered);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct = default)
    {
        return await ExecuteWriteOperationAsync(async () =>
        {
            if (CreateFromBranchId == CreateToBranchId)
                throw new InvalidOperationException("Source and destination branches cannot be the same.");

            var validItems = TransferItems.Where(i => i.ItemId != Guid.Empty && i.RequestedQuantity > 0).ToList();
            if (!validItems.Any())
                throw new InvalidOperationException("Please select at least one item to transfer.");

            var req = new CreateTransferRequest
            {
                FromBranchId = CreateFromBranchId,
                ToBranchId = CreateToBranchId,
                Notes = string.IsNullOrWhiteSpace(CreateNotes) ? null : CreateNotes.Trim(),
                Items = validItems
            };

            var dto = await _transferManager.CreateAsync(req, Guid.Empty, ct);
            StatusMessage = $"Stock Transfer #{dto.StockTransferId} submitted successfully.";
        });
    }

    public async Task<IActionResult> OnPostApproveAsync(CancellationToken ct = default)
    {
        return await ExecuteWriteOperationAsync(async () =>
        {
            var req = new ApproveTransferRequest { Notes = ApproveNotes?.Trim() };
            await _transferManager.ApproveAsync(ActionTransferId, Guid.Empty, req, ct);
            StatusMessage = $"Transfer #{ActionTransferId} approved for dispatch.";
        });
    }

    public async Task<IActionResult> OnPostRejectAsync(CancellationToken ct = default)
    {
        return await ExecuteWriteOperationAsync(async () =>
        {
            var req = new RejectTransferRequest { Reason = RejectReason?.Trim() ?? "Rejected by branch supervisor" };
            await _transferManager.RejectAsync(ActionTransferId, Guid.Empty, req, ct);
            StatusMessage = $"Transfer #{ActionTransferId} rejected.";
        });
    }

    public async Task<IActionResult> OnPostDispatchAsync(CancellationToken ct = default)
    {
        return await ExecuteWriteOperationAsync(async () =>
        {
            var req = new DispatchTransferRequest
            {
                Notes = DispatchNotes?.Trim(),
                Items = DispatchItems.Select(i => new DispatchItemLine
                {
                    StockTransferItemId = i.StockTransferItemId,
                    DispatchedQuantity = i.DispatchedQuantity
                }).ToList()
            };

            await _transferManager.DispatchAsync(ActionTransferId, Guid.Empty, req, ct);
            StatusMessage = $"Transfer #{ActionTransferId} marked as Dispatched (In Transit). Origin stock deducted.";
        });
    }

    public async Task<IActionResult> OnPostReceiveAsync(CancellationToken ct = default)
    {
        return await ExecuteWriteOperationAsync(async () =>
        {
            var req = new ReceiveTransferRequest
            {
                Notes = ReceiveNotes?.Trim(),
                Items = ReceiveItems.Select(i => new ReceiveItemLine
                {
                    StockTransferItemId = i.StockTransferItemId,
                    ReceivedQuantity = i.ReceivedQuantity
                }).ToList()
            };

            await _transferManager.ReceiveAsync(ActionTransferId, Guid.Empty, req, ct);
            StatusMessage = $"Transfer #{ActionTransferId} confirmed Received. Destination stock credited.";
        });
    }

    public async Task<IActionResult> OnPostCancelAsync(CancellationToken ct = default)
    {
        return await ExecuteWriteOperationAsync(async () =>
        {
            await _transferManager.CancelAsync(ActionTransferId, Guid.Empty, CancelReason?.Trim(), ct);
            StatusMessage = $"Transfer #{ActionTransferId} cancelled.";
        });
    }

    public async Task<IActionResult> OnGetExportCsvAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        _apiClient.SetToken(token);
        if (!HasPermission(permissions, PermissionKeys.InventoryRead))
            return AccessDenied();

        var result = await _transferManager.GetTransfersPagedAsync(new TransferFilterRequest
        {
            Page = 1,
            PageSize = 5000,
            BranchId = BranchId,
            Status = Status,
            SearchTerm = Search
        }, ct);

        var sb = new StringBuilder();
        sb.AppendLine("Transfer ID,Status,From Branch,To Branch,Total Lines,Requested Units,Dispatched Units,Received Units,Valuation (XAF),Requested By,Date Created,Approved At,Dispatched At,Received At,Notes");

        foreach (var t in result.Items)
        {
            sb.AppendLine($"\"#TRF-{t.StockTransferId}\",\"{t.Status}\",\"{EscapeCsv(t.FromBranchName)}\",\"{EscapeCsv(t.ToBranchName)}\",\"{t.Items.Count}\",\"{t.TotalRequestedUnits}\",\"{t.TotalDispatchedUnits}\",\"{t.TotalReceivedUnits}\",\"{t.TotalValuation:F2}\",\"{EscapeCsv(t.RequestedByUser)}\",\"{t.DateCreated:yyyy-MM-dd HH:mm}\",\"{(t.ApprovedAt.HasValue ? t.ApprovedAt.Value.ToString("yyyy-MM-dd HH:mm") : "")}\",\"{(t.DispatchedAt.HasValue ? t.DispatchedAt.Value.ToString("yyyy-MM-dd HH:mm") : "")}\",\"{(t.ReceivedAt.HasValue ? t.ReceivedAt.Value.ToString("yyyy-MM-dd HH:mm") : "")}\",\"{EscapeCsv(t.Notes)}\"");
        }

        var fileName = $"stock_transfers_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
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

    public class DispatchItemInput
    {
        public int StockTransferItemId { get; set; }
        public int DispatchedQuantity { get; set; }
    }

    public class ReceiveItemInput
    {
        public int StockTransferItemId { get; set; }
        public int ReceivedQuantity { get; set; }
    }
}

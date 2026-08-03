using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;
using Store.Models.DTOs.Transfers;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class StockTransfersModel : SecurePageModel
{
    private readonly IStockTransferService _transferService;
    private readonly IItemService _itemService;
    private readonly IApiClientService _apiClient;

    public List<StockTransferDto> Transfers { get; private set; } = new();
    public string? FilterStatus { get; private set; }

    // Create
    [BindProperty] public int CreateFromBranchId { get; set; }
    [BindProperty] public int CreateToBranchId { get; set; }
    [BindProperty] public string? CreateNotes { get; set; }
    [BindProperty] public List<TransferItemLine> TransferItems { get; set; } = new();

    // Shared action field
    [BindProperty] public int ActionTransferId { get; set; }

    // Approve
    [BindProperty] public string? ApproveNotes { get; set; }

    // Reject
    [BindProperty] public string? RejectReason { get; set; }

    // Dispatch
    [BindProperty] public List<DispatchItemInput> DispatchItems { get; set; } = new();
    [BindProperty] public string? DispatchNotes { get; set; }

    // Receive
    [BindProperty] public List<ReceiveItemInput> ReceiveItems { get; set; } = new();
    [BindProperty] public string? ReceiveNotes { get; set; }

    // Cancel
    [BindProperty] public string? CancelReason { get; set; }

    [TempData] public string? StatusMessage { get; set; }

    public StockTransfersModel(
        IStockTransferService transferService,
        IItemService itemService,
        IApiClientService apiClient)
    {
        _transferService = transferService;
        _itemService = itemService;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync([FromQuery] string? status = null)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        FilterStatus = status;
        Transfers = await _transferService.GetAllAsync(status: status);
        return Page();
    }

    public async Task<IActionResult> OnGetSearchItemsAsync([FromQuery] string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);
        var result = await _itemService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 15, SearchTerm = q?.Trim() }, ct);
        var items = result.Items.Select(i => new
        {
            id = i.ItemId.ToString(),
            title = i.Name,
            sub = $"Barcode: {(string.IsNullOrEmpty(i.Barcode) ? "N/A" : i.Barcode)} | Unit: {(string.IsNullOrEmpty(i.UnitAbbreviation) ? "Unit" : i.UnitAbbreviation)}",
            badge = i.CategoryName ?? "Item"
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
            .Where(b => string.IsNullOrEmpty(search) ||
                        b.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        b.Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        b.BranchId.ToString() == search)
            .Take(15)
            .Select(b => new
            {
                id = b.BranchId.ToString(),
                title = b.Name,
                sub = $"Code: {b.Code} | #{b.BranchId}",
                badge = b.IsActive ? "Active" : "Inactive"
            });

        return new JsonResult(filtered);
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        if (!ModelState.IsValid)
        {
            StatusMessage = "Error: Invalid input data. Please check your form values.";
            return RedirectToPage();
        }

        _apiClient.SetToken(token);

        var req = new CreateTransferRequest
        {
            FromBranchId = CreateFromBranchId,
            ToBranchId = CreateToBranchId,
            Notes = string.IsNullOrWhiteSpace(CreateNotes) ? null : CreateNotes.Trim(),
            Items = TransferItems
        };

        try
        {
            var dto = await _transferService.CreateAsync(req, Guid.Empty);
            StatusMessage = $"Transfer #{dto.StockTransferId} submitted.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostApproveAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var req = new ApproveTransferRequest { Notes = ApproveNotes?.Trim() };
        await _transferService.ApproveAsync(ActionTransferId, Guid.Empty, req);
        StatusMessage = $"Transfer #{ActionTransferId} approved.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var req = new RejectTransferRequest { Reason = RejectReason ?? "No reason provided" };
        await _transferService.RejectAsync(ActionTransferId, Guid.Empty, req);
        StatusMessage = $"Transfer #{ActionTransferId} rejected.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDispatchAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var req = new DispatchTransferRequest
        {
            Notes = DispatchNotes?.Trim(),
            Items = DispatchItems.Select(i => new DispatchItemLine
            {
                StockTransferItemId = i.StockTransferItemId,
                DispatchedQuantity = i.DispatchedQuantity
            }).ToList()
        };

        await _transferService.DispatchAsync(ActionTransferId, Guid.Empty, req);
        StatusMessage = $"Transfer #{ActionTransferId} dispatched.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReceiveAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var req = new ReceiveTransferRequest
        {
            Notes = ReceiveNotes?.Trim(),
            Items = ReceiveItems.Select(i => new ReceiveItemLine
            {
                StockTransferItemId = i.StockTransferItemId,
                ReceivedQuantity = i.ReceivedQuantity
            }).ToList()
        };

        await _transferService.ReceiveAsync(ActionTransferId, Guid.Empty, req);
        StatusMessage = $"Transfer #{ActionTransferId} received.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        await _transferService.CancelAsync(ActionTransferId, Guid.Empty, CancelReason?.Trim());
        StatusMessage = $"Transfer #{ActionTransferId} cancelled.";
        return RedirectToPage();
    }

    // Input models for form binding
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

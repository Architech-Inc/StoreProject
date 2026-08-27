using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Procurement;
using Store.Models.Enums;
using StoreUI.Services;

namespace StoreUI.Pages;

public class PurchaseOrdersModel : SecurePageModel
{
    private readonly IPurchaseOrderManager _poManager;
    private readonly IApiClientService _apiClient;

    public PurchaseOrderMetricsDto Metrics { get; private set; } = new();
    public PagedResult<PurchaseOrderDto> OrdersPaged { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? SupplierFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty]
    public CreatePurchaseOrderRequest CreateRequest { get; set; } = new();

    [BindProperty]
    public int ActionPurchaseOrderId { get; set; }

    [BindProperty]
    public ReceivePurchaseOrderRequest ReceiveRequest { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public IEnumerable<PurchaseOrderStatus> Statuses { get; } = Enum.GetValues<PurchaseOrderStatus>();

    public PurchaseOrdersModel(IPurchaseOrderManager poManager, IApiClientService apiClient)
    {
        _poManager = poManager;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        Metrics = await _poManager.GetMetricsAsync(ct);

        var filter = new PurchaseOrderFilterRequest
        {
            Page = PageNumber < 1 ? 1 : PageNumber,
            PageSize = 20,
            Status = string.IsNullOrWhiteSpace(StatusFilter) ? null : StatusFilter,
            SupplierId = SupplierFilter,
            SearchTerm = Search
        };

        OrdersPaged = await _poManager.GetPurchaseOrdersPagedAsync(filter, ct);
        return Page();
    }

    public async Task<IActionResult> OnGetDetailsJsonAsync(int id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var po = await _poManager.GetPurchaseOrderByIdAsync(id, ct);
        if (po is null) return NotFound();

        return new JsonResult(po);
    }

    public async Task<IActionResult> OnGetExportCsvAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var filter = new PurchaseOrderFilterRequest
        {
            Page = 1,
            PageSize = 1000,
            Status = string.IsNullOrWhiteSpace(StatusFilter) ? null : StatusFilter,
            SupplierId = SupplierFilter,
            SearchTerm = Search
        };

        var paged = await _poManager.GetPurchaseOrdersPagedAsync(filter, ct);
        var bytes = _poManager.ExportCsv(paged.Items);
        var filename = $"purchase_orders_report_{DateTime.UtcNow:yyyyMMdd_HHmm}.csv";
        return File(bytes, "text/csv", filename);
    }

    public async Task<IActionResult> OnGetSearchSuppliersAsync(string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var suppliers = await _poManager.SearchSuppliersAsync(q, ct);
        var list = suppliers.Select(s => new
        {
            id = s.SupplierId.ToString(),
            name = s.Name,
            contact = s.Emails.FirstOrDefault()?.Email ?? s.Phones.FirstOrDefault()?.PhoneNumber ?? "No contact",
            reg = s.RegistrationNumber ?? "N/A"
        });

        return new JsonResult(list);
    }

    public async Task<IActionResult> OnGetSearchBranchesAsync(string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var branches = await _poManager.SearchBranchesAsync(q, ct);
        var list = branches.Select(b => new
        {
            id = b.BranchId,
            name = b.Name,
            code = b.Code
        });

        return new JsonResult(list);
    }

    public async Task<IActionResult> OnGetSearchCatalogAsync(string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);

        var items = await _poManager.SearchCatalogItemsAsync(q, ct);
        var list = items.Select(i => new
        {
            id = i.ItemId.ToString(),
            name = i.Name,
            barcode = i.Barcode ?? "N/A",
            category = i.CategoryName ?? "General",
            inStock = i.InStock,
            costPrice = i.CostPrice ?? 0,
            unitPrice = i.UnitPrice
        });

        return new JsonResult(list);
    }

    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        if (CreateRequest.SupplierId == Guid.Empty || CreateRequest.Items == null || !CreateRequest.Items.Any())
        {
            StatusMessage = "Error: Please select a supplier and add at least one line item.";
            return RedirectToPage(new { Search, StatusFilter, SupplierFilter, PageNumber });
        }

        try
        {
            await _poManager.CreatePurchaseOrderAsync(CreateRequest, Guid.Empty, ct);
            StatusMessage = "Purchase order created successfully in Draft status.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Failed to create purchase order - {ex.Message}";
        }

        return RedirectToPage(new { Search, StatusFilter, SupplierFilter, PageNumber });
    }

    public async Task<IActionResult> OnPostSubmitAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        try
        {
            var result = await _poManager.SubmitPurchaseOrderAsync(ActionPurchaseOrderId, Guid.Empty, ct);
            StatusMessage = result is not null ? "Purchase order submitted for manager approval." : "Error: PO must be in Draft status to submit.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Failed to submit purchase order - {ex.Message}";
        }

        return RedirectToPage(new { Search, StatusFilter, SupplierFilter, PageNumber });
    }

    public async Task<IActionResult> OnPostApproveAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        try
        {
            var result = await _poManager.ApprovePurchaseOrderAsync(ActionPurchaseOrderId, Guid.Empty, ct);
            StatusMessage = result is not null ? "Purchase order approved successfully." : "Error: PO must be in Submitted status to approve.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Failed to approve purchase order - {ex.Message}";
        }

        return RedirectToPage(new { Search, StatusFilter, SupplierFilter, PageNumber });
    }

    public async Task<IActionResult> OnPostReceiveAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        if (ReceiveRequest.Lines == null || !ReceiveRequest.Lines.Any(l => l.ReceivedQuantity > 0))
        {
            StatusMessage = "Error: Please specify at least one line item quantity to receive.";
            return RedirectToPage(new { Search, StatusFilter, SupplierFilter, PageNumber });
        }

        try
        {
            var result = await _poManager.ReceivePurchaseOrderAsync(ActionPurchaseOrderId, ReceiveRequest, Guid.Empty, ct);
            StatusMessage = result is not null ? "Goods received successfully and catalog stock updated." : "Error: PO must be in Approved or PartiallyReceived status.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Failed to receive purchase order - {ex.Message}";
        }

        return RedirectToPage(new { Search, StatusFilter, SupplierFilter, PageNumber });
    }

    public async Task<IActionResult> OnPostCancelAsync(CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        try
        {
            var result = await _poManager.CancelPurchaseOrderAsync(ActionPurchaseOrderId, Guid.Empty, ct);
            StatusMessage = result is not null ? "Purchase order cancelled." : "Error: Only Draft or Submitted orders can be cancelled.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Failed to cancel purchase order - {ex.Message}";
        }

        return RedirectToPage(new { Search, StatusFilter, SupplierFilter, PageNumber });
    }
}

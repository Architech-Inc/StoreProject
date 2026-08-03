using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;
using Store.Models.DTOs.Procurement;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

namespace StoreUI.Pages;

public class PurchaseOrdersModel : SecurePageModel
{
    private readonly IPurchaseOrderService _poService;
    private readonly ISupplierService _supplierService;
    private readonly IItemService _itemService;
    private readonly IApiClientService _apiClient;

    public List<PurchaseOrderDto> PurchaseOrders { get; private set; } = new();
    public string? FilterStatus { get; private set; }

    // ---- Create PO ----
    [BindProperty] public Guid CreateSupplierId { get; set; }
    [BindProperty] public int? CreateBranchId { get; set; }
    [BindProperty] public string? CreateReferenceNumber { get; set; }
    [BindProperty] public DateTime? CreateExpectedDelivery { get; set; }
    [BindProperty] public string? CreateNotes { get; set; }

    // Items passed as parallel arrays
    [BindProperty] public List<Guid> CreateItemIds { get; set; } = new();
    [BindProperty] public List<int> CreateQuantities { get; set; } = new();
    [BindProperty] public List<decimal> CreateUnitCosts { get; set; } = new();

    // ---- Action targets ----
    [BindProperty] public int ActionPurchaseOrderId { get; set; }

    // ---- Receive ----
    [BindProperty] public List<int> ReceiveItemLineIds { get; set; } = new();
    [BindProperty] public List<int> ReceiveQuantities { get; set; } = new();

    [TempData] public string? StatusMessage { get; set; }

    public IEnumerable<PurchaseOrderStatus> Statuses { get; } = Enum.GetValues<PurchaseOrderStatus>();

    public PurchaseOrdersModel(
        IPurchaseOrderService poService,
        ISupplierService supplierService,
        IItemService itemService,
        IApiClientService apiClient)
    {
        _poService = poService;
        _supplierService = supplierService;
        _itemService = itemService;
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetSearchSuppliersAsync([FromQuery] string? q, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return Unauthorized();

        _apiClient.SetToken(token);
        var suppliers = await _supplierService.GetAllAsync();
        var search = q?.Trim() ?? string.Empty;
        var filtered = suppliers
            .Where(s => string.IsNullOrEmpty(search) ||
                        s.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (s.RegistrationNumber != null && s.RegistrationNumber.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        s.Emails.Any(e => e.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        s.Phones.Any(p => p.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase)))
            .Take(15)
            .Select(s => new
            {
                id = s.SupplierId.ToString(),
                title = s.Name,
                sub = $"Reg: {s.RegistrationNumber ?? "—"} | Phone: {s.Phones.FirstOrDefault()?.PhoneNumber ?? "—"}",
                badge = s.Emails.FirstOrDefault()?.Email ?? "Supplier"
            });

        return new JsonResult(filtered);
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
            sub = $"Code: {(string.IsNullOrEmpty(i.Barcode) ? "N/A" : i.Barcode)} | Cost: ${i.CostPrice:N2}",
            badge = i.CategoryName ?? "Item",
            cost = i.CostPrice
        });

        return new JsonResult(items);
    }

    public async Task<IActionResult> OnGetAsync([FromQuery] string? status = null)
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);
        FilterStatus = status;

        PurchaseOrderStatus? parsed = null;
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<PurchaseOrderStatus>(status, true, out var s))
            parsed = s;

        PurchaseOrders = await _poService.GetAllAsync(parsed);
        ViewData["ActivePage"] = "PurchaseOrders";
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var lines = new List<CreatePurchaseOrderItemRequest>();
        for (int i = 0; i < CreateItemIds.Count; i++)
        {
            lines.Add(new CreatePurchaseOrderItemRequest
            {
                ItemId = CreateItemIds[i],
                OrderedQuantity = CreateQuantities.ElementAtOrDefault(i),
                UnitCost = CreateUnitCosts.ElementAtOrDefault(i)
            });
        }

        var req = new CreatePurchaseOrderRequest
        {
            SupplierId = CreateSupplierId,
            BranchId = CreateBranchId,
            ReferenceNumber = CreateReferenceNumber,
            ExpectedDeliveryDate = CreateExpectedDelivery,
            Notes = CreateNotes,
            Items = lines
        };

        try
        {
            await _poService.CreateAsync(req, Guid.Empty); // userId injected by API from JWT
            StatusMessage = "Purchase order created successfully.";
        }
        catch
        {
            StatusMessage = "Error: Failed to create purchase order.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSubmitAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var result = await _poService.SubmitAsync(ActionPurchaseOrderId, Guid.Empty);
        StatusMessage = result is not null
            ? "Purchase order submitted for approval."
            : "Error: Could not submit — must be in Draft status.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostApproveAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var result = await _poService.ApproveAsync(ActionPurchaseOrderId, Guid.Empty);
        StatusMessage = result is not null
            ? "Purchase order approved."
            : "Error: Could not approve — must be in Submitted status.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReceiveAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var lines = new List<ReceiveItemLine>();
        for (int i = 0; i < ReceiveItemLineIds.Count; i++)
        {
            lines.Add(new ReceiveItemLine
            {
                PurchaseOrderItemId = ReceiveItemLineIds[i],
                ReceivedQuantity = ReceiveQuantities.ElementAtOrDefault(i)
            });
        }

        var result = await _poService.ReceiveAsync(ActionPurchaseOrderId, new ReceivePurchaseOrderRequest { Lines = lines }, Guid.Empty);
        StatusMessage = result is not null
            ? "Goods received and stock updated."
            : "Error: Could not receive — PO must be Approved or PartiallyReceived.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        if (!TryGetSecurityContext(out var token, out _))
            return GoToLogin();

        _apiClient.SetToken(token);

        var result = await _poService.CancelAsync(ActionPurchaseOrderId, Guid.Empty);
        StatusMessage = result is not null
            ? "Purchase order cancelled."
            : "Error: Could not cancel — only Draft or Submitted orders can be cancelled.";

        return RedirectToPage();
    }
}

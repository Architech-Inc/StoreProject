using System.Text;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Items;
using Store.Models.DTOs.Operations;
using Store.Models.DTOs.Procurement;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class PurchaseOrderManager : IPurchaseOrderManager
{
    private readonly IPurchaseOrderService _poService;
    private readonly ISupplierService _supplierService;
    private readonly IItemService _itemService;
    private readonly IApiClientService _apiClient;

    public PurchaseOrderManager(
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

    public async Task<PurchaseOrderMetricsDto> GetMetricsAsync(CancellationToken ct = default)
        => await _poService.GetPurchaseOrderMetricsAsync(ct);

    public async Task<PagedResult<PurchaseOrderDto>> GetPurchaseOrdersPagedAsync(PurchaseOrderFilterRequest request, CancellationToken ct = default)
        => await _poService.GetPurchaseOrdersPagedAsync(request, ct);

    public async Task<PurchaseOrderDto?> GetPurchaseOrderByIdAsync(int id, CancellationToken ct = default)
        => await _poService.GetByIdAsync(id);

    public async Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderRequest request, Guid userId, CancellationToken ct = default)
        => await _poService.CreateAsync(request, userId);

    public async Task<PurchaseOrderDto?> SubmitPurchaseOrderAsync(int id, Guid userId, CancellationToken ct = default)
        => await _poService.SubmitAsync(id, userId);

    public async Task<PurchaseOrderDto?> ApprovePurchaseOrderAsync(int id, Guid userId, CancellationToken ct = default)
        => await _poService.ApproveAsync(id, userId);

    public async Task<PurchaseOrderDto?> ReceivePurchaseOrderAsync(int id, ReceivePurchaseOrderRequest request, Guid userId, CancellationToken ct = default)
        => await _poService.ReceiveAsync(id, request, userId);

    public async Task<PurchaseOrderDto?> CancelPurchaseOrderAsync(int id, Guid userId, CancellationToken ct = default)
        => await _poService.CancelAsync(id, userId);

    public async Task<List<SupplierDto>> SearchSuppliersAsync(string? query, CancellationToken ct = default)
    {
        var suppliers = await _supplierService.GetAllAsync();
        var search = query?.Trim() ?? string.Empty;
        return suppliers
            .Where(s => string.IsNullOrEmpty(search) ||
                        s.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (s.RegistrationNumber != null && s.RegistrationNumber.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        s.Emails.Any(e => e.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        s.Phones.Any(p => p.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase)))
            .Take(20)
            .ToList();
    }

    public async Task<List<BranchDto>> SearchBranchesAsync(string? query, CancellationToken ct = default)
    {
        var branches = await _apiClient.GetAsync<List<BranchDto>>("api/admin/branches", ct) ?? new();
        var search = query?.Trim() ?? string.Empty;
        return branches
            .Where(b => string.IsNullOrEmpty(search) ||
                        b.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        b.Code.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        b.BranchId.ToString() == search)
            .Take(20)
            .ToList();
    }

    public async Task<List<ItemDto>> SearchCatalogItemsAsync(string? query, CancellationToken ct = default)
    {
        var req = new PagedRequest
        {
            Page = 1,
            PageSize = 25,
            SearchTerm = query?.Trim()
        };
        var result = await _itemService.GetAllAsync(req, ct);
        return result.Items.ToList();
    }

    public byte[] ExportCsv(IEnumerable<PurchaseOrderDto> orders)
    {
        var sb = new StringBuilder();
        sb.AppendLine("PO ID,Reference Number,Date Created,Supplier Name,Supplier Contact,Branch,Status,Expected Delivery,Line Count,Ordered Units,Received Units,Total Valuation (XAF),Requested By,Approved By,Received Date,Notes");

        foreach (var po in orders)
        {
            sb.AppendLine(string.Join(",",
                po.PurchaseOrderId,
                EscapeCsv(po.ReferenceNumber ?? $"PO-{po.PurchaseOrderId}"),
                EscapeCsv(po.DateCreated.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")),
                EscapeCsv(po.SupplierName),
                EscapeCsv(po.SupplierEmail ?? po.SupplierPhone ?? "—"),
                EscapeCsv(po.BranchName ?? "Main Warehouse"),
                EscapeCsv(po.Status),
                EscapeCsv(po.ExpectedDeliveryDate?.ToString("yyyy-MM-dd") ?? "—"),
                po.LineCount,
                po.TotalOrderedUnits,
                po.TotalReceivedUnits,
                po.TotalValuation.ToString("F2"),
                EscapeCsv(po.RequestedByUser),
                EscapeCsv(po.ApprovedByUser ?? "—"),
                EscapeCsv(po.ReceivedAt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "—"),
                EscapeCsv(po.Notes ?? "")
            ));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "\"\"";
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }
        return $"\"{field}\"";
    }
}

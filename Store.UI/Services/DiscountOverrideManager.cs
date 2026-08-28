using System.Text;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Discounts;
using Store.Models.DTOs.Invoices;
using Store.Models.DTOs.Items;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class DiscountOverrideManager : IDiscountOverrideManager
{
    private readonly IDiscountOverrideService _overrideService;
    private readonly IInvoiceService _invoiceService;
    private readonly IItemService _itemService;

    public DiscountOverrideManager(
        IDiscountOverrideService overrideService,
        IInvoiceService invoiceService,
        IItemService itemService)
    {
        _overrideService = overrideService;
        _invoiceService = invoiceService;
        _itemService = itemService;
    }

    public async Task<DiscountOverrideMetricsDto> GetMetricsAsync(CancellationToken ct = default)
        => await _overrideService.GetMetricsAsync(ct);

    public async Task<PagedResult<DiscountOverrideDto>> GetOverridesPagedAsync(DiscountOverrideFilterRequest request, CancellationToken ct = default)
        => await _overrideService.GetOverridesPagedAsync(request, ct);

    public async Task<DiscountOverrideDto?> GetOverrideByIdAsync(int id, CancellationToken ct = default)
        => await _overrideService.GetByIdAsync(id);

    public async Task<DiscountOverrideDto> CreateOverrideAsync(CreateDiscountOverrideRequest request, Guid requestedByUserId, CancellationToken ct = default)
        => await _overrideService.CreateAsync(request, requestedByUserId);

    public async Task<DiscountOverrideDto?> ReviewOverrideAsync(int id, Guid reviewedByUserId, ReviewDiscountOverrideRequest request, CancellationToken ct = default)
        => await _overrideService.ReviewAsync(id, reviewedByUserId, request);

    public async Task<bool> CancelOverrideAsync(int id, Guid userId, CancellationToken ct = default)
        => await _overrideService.CancelAsync(id, userId);

    public async Task<List<InvoiceDto>> SearchInvoicesAsync(string? query, CancellationToken ct = default)
    {
        var result = await _invoiceService.GetAllAsync(new PagedRequest { Page = 1, PageSize = 25 }, ct);
        var q = query?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(q))
            return result.Items.ToList();

        return result.Items
            .Where(inv => (inv.CustomerName?.ToLowerInvariant().Contains(q) == true) ||
                          inv.InvoiceId.ToString().ToLowerInvariant().Contains(q))
            .ToList();
    }

    public async Task<List<ItemDto>> SearchItemsAsync(string? query, CancellationToken ct = default)
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

    public byte[] ExportCsv(IEnumerable<DiscountOverrideDto> overrides)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Request ID,Created Date,Scope,Target,Type,Value,Est Impact (XAF),Status,Cashier Username,Cashier Name,Supervisor,Review Date,Justification,Review Notes");

        foreach (var r in overrides)
        {
            sb.AppendLine(string.Join(",",
                r.DiscountOverrideRequestId,
                EscapeCsv(r.DateCreated.ToString("yyyy-MM-dd HH:mm")),
                EscapeCsv(r.ScopeType),
                EscapeCsv(r.ScopeLabel),
                EscapeCsv(r.OverrideType),
                EscapeCsv(r.ValueFormatted),
                r.EstimatedImpactXaf.ToString("F2"),
                EscapeCsv(r.Status),
                EscapeCsv(r.RequestedByUser),
                EscapeCsv(r.RequestedByFullName ?? ""),
                EscapeCsv(r.ReviewedByUser ?? "—"),
                EscapeCsv(r.ReviewedAt?.ToString("yyyy-MM-dd HH:mm") ?? "—"),
                EscapeCsv(r.Justification ?? ""),
                EscapeCsv(r.ReviewNotes ?? "")
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

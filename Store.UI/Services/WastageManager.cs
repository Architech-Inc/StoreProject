using System.Text;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Inventory;
using Store.Models.DTOs.Items;
using Store.Models.Interfaces.Services;

namespace StoreUI.Services;

public class WastageManager : IWastageManager
{
    private readonly IWastageService _wastageService;
    private readonly IItemService _itemService;

    public WastageManager(IWastageService wastageService, IItemService itemService)
    {
        _wastageService = wastageService;
        _itemService = itemService;
    }

    public async Task<WastageMetricsDto> GetMetricsAsync(CancellationToken ct = default)
        => await _wastageService.GetWastageMetricsAsync(ct);

    public async Task<PagedResult<WastageEntryDto>> GetWastagePagedAsync(WastageFilterRequest request, CancellationToken ct = default)
        => await _wastageService.GetWastagePagedAsync(request, ct);

    public async Task<WastageEntryDto> RecordWastageAsync(RecordWastageRequest request, Guid recordedByUserId, CancellationToken ct = default)
        => await _wastageService.RecordAsync(request, recordedByUserId);

    public async Task<bool> DeleteWastageAsync(int id, CancellationToken ct = default)
        => await _wastageService.DeleteAsync(id);

    public async Task<List<ItemDto>> SearchCatalogItemsAsync(string? query, CancellationToken ct = default)
    {
        var req = new PagedRequest
        {
            SearchTerm = query?.Trim(),
            PageSize = 20
        };
        var result = await _itemService.GetAllAsync(req, ct);
        return result.Items.ToList();
    }

    public byte[] ExportCsv(IEnumerable<WastageEntryDto> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Entry ID,Reference Code,Date,Item Name,Barcode,Category,Wastage Reason,Quantity,Unit Cost (XAF),Total Loss (XAF),Recorded By,Notes");

        foreach (var e in entries)
        {
            sb.AppendLine(string.Join(",",
                e.WastageEntryId,
                EscapeCsv(e.ReferenceCode ?? $"WASTE-{e.WastageEntryId}"),
                EscapeCsv(e.DateCreated.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")),
                EscapeCsv(e.ItemName),
                EscapeCsv(e.ItemCode),
                EscapeCsv(e.CategoryName ?? "General"),
                EscapeCsv(e.WastageType),
                e.Quantity,
                e.UnitCost.ToString("F2"),
                e.LineValuation.ToString("F2"),
                EscapeCsv(e.RecordedByUser),
                EscapeCsv(e.Notes ?? "")
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

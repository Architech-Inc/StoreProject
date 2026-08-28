using Store.Models.DTOs.Operations;

namespace StoreUI.Services;

public interface IPromotionEffectivenessManager
{
    Task<PromotionEffectivenessDto?> GetEffectivenessReportAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default);
    byte[] GenerateCsv(PromotionEffectivenessDto report, string section);
}

using Store.Models.DTOs.Operations;

namespace StoreUI.Services;

public interface IReconciliationManager
{
    Task<DayEndReconciliationDto?> GetDayEndReconciliationAsync(DateOnly date, CancellationToken ct = default);
    byte[] GenerateReconciliationCsv(DayEndReconciliationDto report);
}

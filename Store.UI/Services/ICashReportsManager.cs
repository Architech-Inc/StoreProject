using Store.Models.DTOs.Operations;

namespace StoreUI.Services;

public interface ICashReportsManager
{
    Task<CashierShiftDto?> GetActiveShiftAsync(CancellationToken ct = default);
    Task<List<CashierShiftDto>> GetShiftsAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<CashierShiftDto?> OpenShiftAsync(ShiftOpenRequest request, CancellationToken ct = default);
    Task<CashierShiftDto?> CloseShiftAsync(ShiftCloseRequest request, CancellationToken ct = default);
    Task<DailyZReportDto?> GetDailyZReportAsync(DateTime dateUtc, CancellationToken ct = default);
    byte[] GenerateZReportCsv(DailyZReportDto report);
}

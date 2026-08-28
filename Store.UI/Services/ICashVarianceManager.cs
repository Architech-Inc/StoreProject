using Store.Models.DTOs.Cash;
using Store.Models.DTOs.Operations;
using Store.Models.Enums;

namespace StoreUI.Services;

public interface ICashVarianceManager
{
    Task<CashVarianceMetricsDto> GetMetricsAsync(CancellationToken ct = default);
    Task<List<CashVarianceDto>> GetAllAsync(CashVarianceStatus? status = null, CancellationToken ct = default);
    Task<CashVarianceDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<CashVarianceDto>> GetByShiftAsync(Guid shiftId, CancellationToken ct = default);
    Task<List<CashierShiftDto>> SearchShiftsAsync(string? query = null, int limit = 30, CancellationToken ct = default);
    Task<CashVarianceDto?> RecordAsync(RecordCashVarianceRequest request, CancellationToken ct = default);
    Task<CashVarianceDto?> ReviewAsync(int id, ReviewCashVarianceRequest request, CancellationToken ct = default);
    byte[] GenerateCsv(List<CashVarianceDto> list, CashVarianceMetricsDto metrics);
}

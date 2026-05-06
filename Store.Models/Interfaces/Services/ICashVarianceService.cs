using Store.Models.DTOs.Cash;
using Store.Models.Enums;

namespace Store.Models.Interfaces.Services;

public interface ICashVarianceService
{
    Task<List<CashVarianceDto>> GetAllAsync(CashVarianceStatus? status = null);

    Task<CashVarianceDto?> GetByIdAsync(int id);

    Task<List<CashVarianceDto>> GetByShiftAsync(Guid cashierShiftId);

    /// <summary>Cashier records a variance when closing their shift.</summary>
    Task<CashVarianceDto> RecordAsync(RecordCashVarianceRequest request, Guid recordedByUserId);

    /// <summary>Manager reviews and signs off on (or escalates) the variance.</summary>
    Task<CashVarianceDto?> ReviewAsync(int id, Guid reviewedByUserId, ReviewCashVarianceRequest request);
}

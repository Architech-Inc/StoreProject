using Store.Models.DTOs.Payments;
using Store.Models.Enums;

namespace Store.Models.Interfaces.Services;

public interface IMobileMoneyService
{
    Task<MobileMoneyTransactionDto> InitiateAsync(InitiateMobileMoneyRequest request, CancellationToken ct = default);
    Task<MobileMoneyTransactionDto?> HandleMtnMomoCallbackAsync(MtnMomoCallbackRequest callback, CancellationToken ct = default);
    Task<MobileMoneyTransactionDto?> HandleOrangeMoneyCallbackAsync(OrangeMoneyCallbackRequest callback, CancellationToken ct = default);
    Task<SettlementReportDto> GetSettlementReportAsync(DateTime fromDateUtc, DateTime toDateUtc, CancellationToken ct = default);
    Task<IReadOnlyList<MobileMoneyTransactionDto>> GetTransactionsAsync(int page, int pageSize, MobileMoneyStatus? status = null, CancellationToken ct = default);
}

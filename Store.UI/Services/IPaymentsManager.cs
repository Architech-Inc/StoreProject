using Store.Models.DTOs.Payments;
using Store.Models.Enums;

namespace StoreUI.Services;

public interface IPaymentsManager
{
    Task<SettlementReportDto?> GetSettlementReportAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default);
    Task<List<MobileMoneyTransactionDto>> GetTransactionsAsync(int page, int pageSize, MobileMoneyStatus? status = null, CancellationToken ct = default);
    Task<MobileMoneyTransactionDto?> QueryTransactionStatusAsync(Guid transactionId, CancellationToken ct = default);
    byte[] GenerateSettlementCsv(SettlementReportDto? report, IEnumerable<MobileMoneyTransactionDto> transactions);
}

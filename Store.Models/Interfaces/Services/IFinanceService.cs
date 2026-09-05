using Store.Models.DTOs.Finance;
using Store.Models.Entities.Finance;

namespace Store.Models.Interfaces.Services;

public interface IFinanceService
{
    Task<JournalEntry> PostJournalEntryAsync(JournalEntry entry, CancellationToken ct = default);
    Task<IEnumerable<Account>> GetChartOfAccountsAsync(CancellationToken ct = default);
    Task SeedDefaultChartOfAccountsAsync(CancellationToken ct = default);
    Task<ProfitAndLossDto> GenerateProfitAndLossAsync(DateTime periodStart, DateTime periodEnd, CancellationToken ct = default);
    Task<BalanceSheetDto> GenerateBalanceSheetAsync(DateTime asOfDate, CancellationToken ct = default);
    Task<AgingReportDto> GetAccountsReceivableAgingAsync(DateTime asOfDate, CancellationToken ct = default);
    Task<AgingReportDto> GetAccountsPayableAgingAsync(DateTime asOfDate, CancellationToken ct = default);
}

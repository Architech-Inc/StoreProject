using Store.Models.Entities.Finance;

namespace Store.Models.Interfaces.Services;

public interface IFinanceService
{
    Task<JournalEntry> PostJournalEntryAsync(JournalEntry entry, CancellationToken ct = default);
    Task<IEnumerable<Account>> GetChartOfAccountsAsync(CancellationToken ct = default);
    Task SeedDefaultChartOfAccountsAsync(CancellationToken ct = default);
}

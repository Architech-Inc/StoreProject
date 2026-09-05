using Microsoft.EntityFrameworkCore;
using Store.Models.Entities.Finance;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class FinanceService : IFinanceService
{
    private readonly IUnitOfWork _uow;

    public FinanceService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<JournalEntry> PostJournalEntryAsync(JournalEntry entry, CancellationToken ct = default)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));
        if (entry.Lines == null || !entry.Lines.Any()) throw new ArgumentException("Journal entry must have lines.");

        decimal totalDebits = entry.Lines.Sum(l => l.DebitAmount);
        decimal totalCredits = entry.Lines.Sum(l => l.CreditAmount);

        if (totalDebits != totalCredits)
        {
            throw new InvalidOperationException($"Journal entry is unbalanced. Debits: {totalDebits}, Credits: {totalCredits}");
        }

        entry.IsPosted = true;
        entry.Date = entry.Date == default ? DateTime.UtcNow : entry.Date;

        await _uow.Repository<JournalEntry>().AddAsync(entry, ct);
        await _uow.SaveChangesAsync(ct);

        return entry;
    }

    public async Task<IEnumerable<Account>> GetChartOfAccountsAsync(CancellationToken ct = default)
    {
        return await _uow.Repository<Account>().GetAllAsync(ct);
    }

    public async Task SeedDefaultChartOfAccountsAsync(CancellationToken ct = default)
    {
        var existingAccounts = await _uow.Repository<Account>().GetAllAsync(ct);
        if (existingAccounts.Any())
            return; // Already seeded

        var defaultAccounts = new List<Account>
        {
            new Account { AccountCode = "1000", Name = "Cash Equivalents", AccountType = AccountType.Asset, Description = "Cash on hand and in bank." },
            new Account { AccountCode = "1200", Name = "Accounts Receivable", AccountType = AccountType.Asset, Description = "Unpaid customer invoices." },
            new Account { AccountCode = "1300", Name = "Inventory", AccountType = AccountType.Asset, Description = "Value of goods in stock." },
            new Account { AccountCode = "2000", Name = "Accounts Payable", AccountType = AccountType.Liability, Description = "Unpaid vendor bills." },
            new Account { AccountCode = "2200", Name = "Sales Tax Payable", AccountType = AccountType.Liability, Description = "Taxes collected to be remitted." },
            new Account { AccountCode = "3000", Name = "Retained Earnings", AccountType = AccountType.Equity, Description = "Cumulative business profit." },
            new Account { AccountCode = "4000", Name = "Sales Revenue", AccountType = AccountType.Revenue, Description = "Income from retail sales." },
            new Account { AccountCode = "5000", Name = "Cost of Goods Sold", AccountType = AccountType.Expense, Description = "Direct cost of products sold." },
            new Account { AccountCode = "5100", Name = "Inventory Shrinkage", AccountType = AccountType.Expense, Description = "Loss due to theft or damage." }
        };

        foreach (var account in defaultAccounts)
        {
            await _uow.Repository<Account>().AddAsync(account, ct);
        }

        await _uow.SaveChangesAsync(ct);
    }
}

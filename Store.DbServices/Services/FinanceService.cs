using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Finance;
using Store.Models.Entities;
using Store.Models.Entities.Finance;
using Store.Models.Enums;
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
        var existingAccounts = (await _uow.Repository<Account>().GetAllAsync(ct)).ToList();
        var existingCodes = existingAccounts.Select(a => a.AccountCode).ToHashSet();

        var defaultAccounts = new List<Account>
        {
            new Account { AccountCode = "1000", Name = "Cash Equivalents", AccountType = AccountType.Asset, Description = "Cash on hand and in bank." },
            new Account { AccountCode = "1200", Name = "Accounts Receivable", AccountType = AccountType.Asset, Description = "Unpaid customer invoices." },
            new Account { AccountCode = "1300", Name = "Inventory", AccountType = AccountType.Asset, Description = "Value of goods in stock." },
            new Account { AccountCode = "2000", Name = "Accounts Payable", AccountType = AccountType.Liability, Description = "Unpaid vendor bills." },
            new Account { AccountCode = "2100", Name = "Payroll Tax Payable", AccountType = AccountType.Liability, Description = "Withheld payroll taxes to be remitted." },
            new Account { AccountCode = "2200", Name = "Sales Tax Payable", AccountType = AccountType.Liability, Description = "Taxes collected to be remitted." },
            new Account { AccountCode = "3000", Name = "Retained Earnings", AccountType = AccountType.Equity, Description = "Cumulative business profit." },
            new Account { AccountCode = "4000", Name = "Sales Revenue", AccountType = AccountType.Revenue, Description = "Income from retail sales." },
            new Account { AccountCode = "5000", Name = "Cost of Goods Sold", AccountType = AccountType.Expense, Description = "Direct cost of products sold." },
            new Account { AccountCode = "5100", Name = "Inventory Shrinkage", AccountType = AccountType.Expense, Description = "Loss due to theft or damage." },
            new Account { AccountCode = "5200", Name = "Salary Expense", AccountType = AccountType.Expense, Description = "Employee salaries, wages, and allowances." }
        };

        bool anyAdded = false;
        foreach (var account in defaultAccounts)
        {
            if (!existingCodes.Contains(account.AccountCode))
            {
                await _uow.Repository<Account>().AddAsync(account, ct);
                anyAdded = true;
            }
        }

        if (anyAdded)
        {
            await _uow.SaveChangesAsync(ct);
        }
    }

    public async Task<ProfitAndLossDto> GenerateProfitAndLossAsync(DateTime periodStart, DateTime periodEnd, CancellationToken ct = default)
    {
        var accounts = await _uow.Repository<Account>().Query()
            .Include(a => a.JournalEntryLines.Where(l => l.JournalEntry!.IsPosted && l.JournalEntry.Date >= periodStart && l.JournalEntry.Date <= periodEnd))
            .ToListAsync(ct);

        var pnl = new ProfitAndLossDto
        {
            PeriodStart = periodStart,
            PeriodEnd = periodEnd
        };

        foreach (var account in accounts)
        {
            // Revenue accounts have credit normal balance
            if (account.AccountType == AccountType.Revenue)
            {
                var balance = account.JournalEntryLines.Sum(l => l.CreditAmount - l.DebitAmount);
                if (balance != 0)
                {
                    pnl.RevenueAccounts.Add(new AccountBalanceDto { AccountCode = account.AccountCode, AccountName = account.Name, Balance = balance });
                    pnl.TotalRevenue += balance;
                }
            }
            // Expense accounts have debit normal balance
            else if (account.AccountType == AccountType.Expense)
            {
                var balance = account.JournalEntryLines.Sum(l => l.DebitAmount - l.CreditAmount);
                if (balance != 0)
                {
                    if (account.Name.Contains("Cost of Goods") || account.Name.Contains("COGS"))
                    {
                        pnl.COGSAccounts.Add(new AccountBalanceDto { AccountCode = account.AccountCode, AccountName = account.Name, Balance = balance });
                        pnl.TotalCOGS += balance;
                    }
                    else
                    {
                        pnl.ExpenseAccounts.Add(new AccountBalanceDto { AccountCode = account.AccountCode, AccountName = account.Name, Balance = balance });
                        pnl.TotalOperatingExpenses += balance;
                    }
                }
            }
        }

        return pnl;
    }

    public async Task<BalanceSheetDto> GenerateBalanceSheetAsync(DateTime asOfDate, CancellationToken ct = default)
    {
        var accounts = await _uow.Repository<Account>().Query()
            .Include(a => a.JournalEntryLines.Where(l => l.JournalEntry!.IsPosted && l.JournalEntry.Date <= asOfDate))
            .ToListAsync(ct);

        var bs = new BalanceSheetDto
        {
            AsOfDate = asOfDate
        };

        foreach (var account in accounts)
        {
            if (account.AccountType == AccountType.Asset)
            {
                var balance = account.JournalEntryLines.Sum(l => l.DebitAmount - l.CreditAmount);
                if (balance != 0)
                {
                    bs.AssetAccounts.Add(new AccountBalanceDto { AccountCode = account.AccountCode, AccountName = account.Name, Balance = balance });
                    bs.TotalAssets += balance;
                }
            }
            else if (account.AccountType == AccountType.Liability)
            {
                var balance = account.JournalEntryLines.Sum(l => l.CreditAmount - l.DebitAmount);
                if (balance != 0)
                {
                    bs.LiabilityAccounts.Add(new AccountBalanceDto { AccountCode = account.AccountCode, AccountName = account.Name, Balance = balance });
                    bs.TotalLiabilities += balance;
                }
            }
            else if (account.AccountType == AccountType.Equity)
            {
                var balance = account.JournalEntryLines.Sum(l => l.CreditAmount - l.DebitAmount);
                if (balance != 0)
                {
                    bs.EquityAccounts.Add(new AccountBalanceDto { AccountCode = account.AccountCode, AccountName = account.Name, Balance = balance });
                    bs.TotalEquity += balance;
                }
            }
            else if (account.AccountType == AccountType.Revenue || account.AccountType == AccountType.Expense)
            {
                // Roll revenue and expenses into Retained Earnings for Balance Sheet (assuming closed period)
                var creditBalance = account.AccountType == AccountType.Revenue 
                    ? account.JournalEntryLines.Sum(l => l.CreditAmount - l.DebitAmount)
                    : account.JournalEntryLines.Sum(l => l.CreditAmount - l.DebitAmount); // expenses have debit normal, so credit is negative

                if (creditBalance != 0)
                {
                    var reAccount = bs.EquityAccounts.FirstOrDefault(a => a.AccountName == "Retained Earnings");
                    if (reAccount == null)
                    {
                        reAccount = new AccountBalanceDto { AccountCode = "3000", AccountName = "Retained Earnings", Balance = 0 };
                        bs.EquityAccounts.Add(reAccount);
                    }
                    reAccount.Balance += creditBalance;
                    bs.TotalEquity += creditBalance;
                }
            }
        }

        return bs;
    }

    public async Task<AgingReportDto> GetAccountsReceivableAgingAsync(DateTime asOfDate, CancellationToken ct = default)
    {
        var unpaidInvoices = await _uow.Repository<Invoice>().Query()
            .Include(i => i.Customer)
            .Where(i => !i.IsPaid && i.TotalAmount > i.AmountTendered && i.DateCreated <= asOfDate)
            .ToListAsync(ct);

        var report = new AgingReportDto
        {
            AsOfDate = asOfDate,
            ReportType = "AR"
        };

        foreach (var invoice in unpaidInvoices)
        {
            var dueDate = invoice.DueDate ?? invoice.DateCreated;
            var daysOverdue = (int)(asOfDate - dueDate).TotalDays;
            var amountDue = invoice.TotalAmount - invoice.AmountTendered;

            var item = new AgingItemDto
            {
                EntityId = invoice.InvoiceId,
                ReferenceNumber = invoice.InvoiceId.ToString().Substring(0, 8),
                PartyName = (invoice.Customer?.FirstName + " " + invoice.Customer?.LastName).Trim(),
                DueDate = dueDate,
                DaysOverdue = daysOverdue,
                Amount = amountDue
            };

            if (daysOverdue <= 0)
            {
                item.Current = amountDue;
                report.TotalCurrent += amountDue;
            }
            else if (daysOverdue <= 30)
            {
                item.Days1To30 = amountDue;
                report.Total1To30Days += amountDue;
            }
            else if (daysOverdue <= 60)
            {
                item.Days31To60 = amountDue;
                report.Total31To60Days += amountDue;
            }
            else if (daysOverdue <= 90)
            {
                item.Days61To90 = amountDue;
                report.Total61To90Days += amountDue;
            }
            else
            {
                item.Over90Days = amountDue;
                report.TotalOver90Days += amountDue;
            }

            report.Items.Add(item);
        }

        return report;
    }

    public async Task<AgingReportDto> GetAccountsPayableAgingAsync(DateTime asOfDate, CancellationToken ct = default)
    {
        var unpaidPOs = await _uow.Repository<PurchaseOrder>().Query()
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .Where(p => !p.IsPaid && p.Status != PurchaseOrderStatus.Draft && p.Status != PurchaseOrderStatus.Cancelled && p.DateCreated <= asOfDate)
            .ToListAsync(ct);

        var report = new AgingReportDto
        {
            AsOfDate = asOfDate,
            ReportType = "AP"
        };

        foreach (var po in unpaidPOs)
        {
            var dueDate = po.ExpectedDeliveryDate ?? po.DateCreated.AddDays(30); // Default 30 days if no due date
            var daysOverdue = (int)(asOfDate - dueDate).TotalDays;
            
            // Assuming no partial payments on POs tracked at the PO level for now, full amount is due
            var amountDue = po.Items.Sum(i => i.ReceivedQuantity * i.UnitCost);
            
            if (amountDue <= 0) continue;

            var item = new AgingItemDto
            {
                IntEntityId = po.PurchaseOrderId,
                ReferenceNumber = po.ReferenceNumber ?? $"PO-{po.PurchaseOrderId}",
                PartyName = po.Supplier?.Name ?? "Unknown Supplier",
                DueDate = dueDate,
                DaysOverdue = daysOverdue,
                Amount = amountDue
            };

            if (daysOverdue <= 0)
            {
                item.Current = amountDue;
                report.TotalCurrent += amountDue;
            }
            else if (daysOverdue <= 30)
            {
                item.Days1To30 = amountDue;
                report.Total1To30Days += amountDue;
            }
            else if (daysOverdue <= 60)
            {
                item.Days31To60 = amountDue;
                report.Total31To60Days += amountDue;
            }
            else if (daysOverdue <= 90)
            {
                item.Days61To90 = amountDue;
                report.Total61To90Days += amountDue;
            }
            else
            {
                item.Over90Days = amountDue;
                report.TotalOver90Days += amountDue;
            }

            report.Items.Add(item);
        }

        return report;
    }
}

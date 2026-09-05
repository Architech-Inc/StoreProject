using Microsoft.EntityFrameworkCore;
using Store.DbServices.Context;
using Store.DbServices.Services.Interfaces;
using Store.Models.Entities.Finance;
using Store.Models.Entities.HR;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class PayrollService : IPayrollService
{
    private readonly StoreDbContext _context;
    private readonly IFinanceService _financeService;

    public PayrollService(StoreDbContext context, IFinanceService financeService)
    {
        _context = context;
        _financeService = financeService;
    }

    public async Task<PayrollRun> DraftPayrollRunAsync(DateTime periodStart, DateTime periodEnd)
    {
        // 1. Check if a draft run already exists for this period
        var existingDraft = await _context.PayrollRuns
            .FirstOrDefaultAsync(r => r.PeriodStartDate == periodStart && r.PeriodEndDate == periodEnd && r.Status == PayrollRunStatus.Draft);
            
        if (existingDraft != null)
            throw new InvalidOperationException("A draft payroll run already exists for this period.");

        // 2. Create the payroll run
        var payrollRun = new PayrollRun
        {
            PeriodStartDate = periodStart,
            PeriodEndDate = periodEnd,
            RunDate = DateTime.UtcNow,
            Status = PayrollRunStatus.Draft,
            DateCreated = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };
        
        _context.PayrollRuns.Add(payrollRun);

        // 3. Get all active employee contracts with their salary info
        var activeContracts = await _context.EmployeeContracts
            .Include(c => c.Salary)
            .Where(c => c.IsActive && c.StartDate <= periodEnd && (c.EndDate == null || c.EndDate >= periodStart))
            .ToListAsync();

        var taxBrackets = await _context.TaxBrackets
            .Where(t => t.IsActive)
            .OrderBy(t => t.MinAmount)
            .ToListAsync();

        // 4. Calculate payslip for each employee
        foreach (var contract in activeContracts)
        {
            if (contract.Salary == null) continue; // Skip if no salary defined

            decimal basicPay = contract.Salary.BasicAmount;
            decimal allowances = contract.Salary.AllowanceAmount ?? 0;
            decimal commissions = 0;

            // Calculate Commissions
            if (contract.CommissionRate > 0 && contract.CommissionBasis != CommissionBasis.None)
            {
                var userIds = await _context.Users
                    .Where(u => u.EmployeeId == contract.EmployeeId)
                    .Select(u => u.UserId)
                    .ToListAsync();

                if (userIds.Any())
                {
                    if (contract.CommissionBasis == CommissionBasis.GrossSales)
                    {
                        var grossSales = await _context.Invoices
                            .Where(i => userIds.Contains(i.UserId ?? Guid.Empty) && i.IsPaid && i.DateCreated >= periodStart && i.DateCreated <= periodEnd)
                            .SumAsync(i => (decimal?)i.TotalAmount) ?? 0;
                        commissions = grossSales * contract.CommissionRate;
                    }
                    else if (contract.CommissionBasis == CommissionBasis.ProfitMargin)
                    {
                        var profit = await _context.Sales
                            .Include(s => s.Invoice)
                            .Include(s => s.Item)
                            .Where(s => userIds.Contains(s.Invoice.UserId ?? Guid.Empty) && s.Invoice.IsPaid && s.DateCreated >= periodStart && s.DateCreated <= periodEnd)
                            .SumAsync(s => (decimal?)((s.UnitPrice - (s.Item.CostPrice ?? s.Item.UnitPrice * 0.7m)) * s.Quantity)) ?? 0;
                        
                        if (profit > 0)
                        {
                            commissions = profit * contract.CommissionRate;
                        }
                    }
                }
            }

            decimal grossPay = basicPay + allowances + commissions;
            decimal taxDeducted = 0;

            if (contract.PayrollType == PayrollType.Taxed)
            {
                decimal taxableAmount = contract.CalculateTaxOnGross ? grossPay : basicPay;
                taxDeducted = CalculateTax(taxableAmount, taxBrackets);
            }

            decimal netPay = grossPay - taxDeducted;

            var payslip = new Payslip
            {
                PayrollRunId = payrollRun.PayrollRunId,
                EmployeeId = contract.EmployeeId,
                BasicPay = basicPay,
                Allowances = allowances,
                Commissions = commissions,
                GrossPay = grossPay,
                TaxDeducted = taxDeducted,
                NetPay = netPay,
                DateCreated = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            _context.Payslips.Add(payslip);

            payrollRun.TotalGross += grossPay;
            payrollRun.TotalAllowances += allowances;
            payrollRun.TotalCommissions += commissions;
            payrollRun.TotalTax += taxDeducted;
            payrollRun.TotalNet += netPay;
        }

        await _context.SaveChangesAsync();
        return payrollRun;
    }

    public async Task<PayrollRun> ApprovePayrollAsync(Guid payrollRunId, Guid approvedByUserId)
    {
        var run = await _context.PayrollRuns.FindAsync(payrollRunId);
        if (run == null) throw new KeyNotFoundException("Payroll run not found.");
        if (run.Status != PayrollRunStatus.Draft) throw new InvalidOperationException("Only draft runs can be approved.");

        run.Status = PayrollRunStatus.Approved;
        run.ApprovedByUserId = approvedByUserId;
        run.ApprovedAt = DateTime.UtcNow;
        run.LastModified = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return run;
    }

    public async Task<bool> PayPayrollRunAsync(Guid payrollRunId)
    {
        var run = await _context.PayrollRuns.FindAsync(payrollRunId);
        if (run == null) throw new KeyNotFoundException("Payroll run not found.");
        if (run.Status != PayrollRunStatus.Approved) throw new InvalidOperationException("Only approved runs can be paid.");

        // Get Chart of Accounts to find relevant accounts
        var coa = await _financeService.GetChartOfAccountsAsync();
        
        // Find specific accounts (fallback to defaults if not found, ideally these would be configured via settings)
        var cashAccount = coa.FirstOrDefault(a => a.Name == "Cash") ?? throw new InvalidOperationException("Cash account not found.");
        var salaryExpenseAccount = coa.FirstOrDefault(a => a.Name == "Salary Expense") ?? throw new InvalidOperationException("Salary Expense account not found.");
        // If we want to record tax liability
        var payrollTaxPayable = coa.FirstOrDefault(a => a.Name == "Payroll Tax Payable"); // Optional

        // Create Journal Entry
        var je = new JournalEntry
        {
            Date = DateTime.UtcNow,
            ReferenceId = run.PayrollRunId.ToString(),
            ReferenceType = Store.Models.Entities.Finance.ReferenceType.Manual, // Using Manual for Payroll as there is no specific Payroll ReferenceType
            Description = $"Payroll payment for period {run.PeriodStartDate:yyyy-MM-dd} to {run.PeriodEndDate:yyyy-MM-dd}"
        };

        // Debit Salary Expense (Gross Pay)
        je.Lines.Add(new JournalEntryLine
        {
            AccountId = salaryExpenseAccount.AccountId,
            DebitAmount = run.TotalGross,
            CreditAmount = 0,
            Description = "Total Gross Salary"
        });

        // Credit Cash (Net Pay sent to employees)
        je.Lines.Add(new JournalEntryLine
        {
            AccountId = cashAccount.AccountId,
            DebitAmount = 0,
            CreditAmount = run.TotalNet,
            Description = "Net Salary Paid"
        });

        // Credit Tax Payable (if we have an account for it, otherwise we just assume it's paid out immediately or booked elsewhere)
        if (payrollTaxPayable != null && run.TotalTax > 0)
        {
            je.Lines.Add(new JournalEntryLine
            {
                AccountId = payrollTaxPayable.AccountId,
                DebitAmount = 0,
                CreditAmount = run.TotalTax,
                Description = "Tax Withheld"
            });
        }
        else if (run.TotalTax > 0)
        {
            // If no tax payable account, just credit cash for the tax as well (assuming it's paid to govt immediately)
            je.Lines.Add(new JournalEntryLine
            {
                AccountId = cashAccount.AccountId,
                DebitAmount = 0,
                CreditAmount = run.TotalTax,
                Description = "Tax Withheld and Paid"
            });
        }

        // Post Journal Entry
        await _financeService.PostJournalEntryAsync(je);

        // Update run status
        run.Status = PayrollRunStatus.Paid;
        run.LastModified = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<PayrollRun>> GetAllPayrollRunsAsync()
    {
        return await _context.PayrollRuns
            .OrderByDescending(r => r.PeriodEndDate)
            .ToListAsync();
    }

    public async Task<PayrollRun?> GetPayrollRunByIdAsync(Guid payrollRunId)
    {
        return await _context.PayrollRuns
            .Include(r => r.Payslips)
            .ThenInclude(p => p.Employee)
            .FirstOrDefaultAsync(r => r.PayrollRunId == payrollRunId);
    }

    public async Task<IEnumerable<Payslip>> GetPayslipsForRunAsync(Guid payrollRunId)
    {
        return await _context.Payslips
            .Include(p => p.Employee)
            .Where(p => p.PayrollRunId == payrollRunId)
            .ToListAsync();
    }

    private decimal CalculateTax(decimal amount, List<TaxBracket> brackets)
    {
        decimal totalTax = 0;
        
        foreach (var bracket in brackets)
        {
            if (amount > bracket.MinAmount)
            {
                decimal amountInBracket = (bracket.MaxAmount.HasValue && amount > bracket.MaxAmount.Value)
                    ? bracket.MaxAmount.Value - bracket.MinAmount
                    : amount - bracket.MinAmount;

                totalTax += bracket.FixedTaxAmount;
                totalTax += amountInBracket * (bracket.TaxPercentage / 100);
            }
        }

        return totalTax;
    }
}

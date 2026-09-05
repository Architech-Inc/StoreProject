namespace Store.Models.DTOs.Finance;

public class ProfitAndLossDto
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCOGS { get; set; }
    public decimal GrossProfit => TotalRevenue - TotalCOGS;
    public decimal TotalOperatingExpenses { get; set; }
    public decimal NetIncome => GrossProfit - TotalOperatingExpenses;

    public List<AccountBalanceDto> RevenueAccounts { get; set; } = new();
    public List<AccountBalanceDto> COGSAccounts { get; set; } = new();
    public List<AccountBalanceDto> ExpenseAccounts { get; set; } = new();
}

public class AccountBalanceDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

using Store.Models.DTOs.Finance;

namespace StoreUI.Services;

public interface IFinanceManager
{
    Task<ProfitAndLossDto?> GetProfitAndLossAsync(DateTime? startDate, DateTime? endDate);
    Task<BalanceSheetDto?> GetBalanceSheetAsync(DateTime? asOfDate);
    Task<AgingReportDto?> GetAccountsReceivableAgingAsync(DateTime? asOfDate);
    Task<AgingReportDto?> GetAccountsPayableAgingAsync(DateTime? asOfDate);
}

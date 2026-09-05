using Store.Models.DTOs.Finance;

namespace StoreUI.Services;

public class FinanceManager : IFinanceManager
{
    private readonly IApiClientService _api;

    public FinanceManager(IApiClientService api)
    {
        _api = api;
    }

    public async Task<ProfitAndLossDto?> GetProfitAndLossAsync(DateTime? startDate, DateTime? endDate)
    {
        var startQuery = startDate.HasValue ? $"startDate={startDate.Value:O}" : "";
        var endQuery = endDate.HasValue ? $"endDate={endDate.Value:O}" : "";
        var query = string.Join("&", new[] { startQuery, endQuery }.Where(q => !string.IsNullOrEmpty(q)));
        var url = $"api/finance/profit-and-loss{(string.IsNullOrEmpty(query) ? "" : "?" + query)}";

        return await _api.GetAsync<ProfitAndLossDto>(url);
    }

    public async Task<BalanceSheetDto?> GetBalanceSheetAsync(DateTime? asOfDate)
    {
        var query = asOfDate.HasValue ? $"?asOfDate={asOfDate.Value:O}" : "";
        var url = $"api/finance/balance-sheet{query}";

        return await _api.GetAsync<BalanceSheetDto>(url);
    }

    public async Task<AgingReportDto?> GetAccountsReceivableAgingAsync(DateTime? asOfDate)
    {
        var query = asOfDate.HasValue ? $"?asOfDate={asOfDate.Value:O}" : "";
        var url = $"api/finance/ar-aging{query}";

        return await _api.GetAsync<AgingReportDto>(url);
    }

    public async Task<AgingReportDto?> GetAccountsPayableAgingAsync(DateTime? asOfDate)
    {
        var query = asOfDate.HasValue ? $"?asOfDate={asOfDate.Value:O}" : "";
        var url = $"api/finance/ap-aging{query}";

        return await _api.GetAsync<AgingReportDto>(url);
    }
}

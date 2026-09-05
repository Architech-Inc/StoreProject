using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Finance;
using StoreUI.Pages;
using StoreUI.Services;

namespace StoreUI.Pages;

public class FinanceModel : SecurePageModel
{
    private readonly IFinanceManager _financeManager;
    private readonly IApiClientService _apiClient;

    public FinanceModel(IFinanceManager financeManager, IApiClientService apiClient)
    {
        _financeManager = financeManager;
        _apiClient = apiClient;
    }

    public ProfitAndLossDto? ProfitAndLoss { get; set; }
    public BalanceSheetDto? BalanceSheet { get; set; }
    public AgingReportDto? ArAging { get; set; }
    public AgingReportDto? ApAging { get; set; }

    [BindProperty(SupportsGet = true)]
    public string ViewType { get; set; } = "PnL"; // "PnL", "BalanceSheet", "ARAging", "APAging"

    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? AsOfDate { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!TryGetSecurityContext(out var token, out _)) return GoToLogin();
        _apiClient.SetToken(token);

        if (ViewType == "PnL")
        {
            ProfitAndLoss = await _financeManager.GetProfitAndLossAsync(StartDate, EndDate);
        }
        else if (ViewType == "BalanceSheet")
        {
            BalanceSheet = await _financeManager.GetBalanceSheetAsync(AsOfDate);
        }
        else if (ViewType == "ARAging")
        {
            ArAging = await _financeManager.GetAccountsReceivableAgingAsync(AsOfDate);
        }
        else if (ViewType == "APAging")
        {
            ApAging = await _financeManager.GetAccountsPayableAgingAsync(AsOfDate);
        }

        return Page();
    }
}

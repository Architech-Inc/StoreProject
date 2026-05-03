using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using Store.Models.DTOs.Payments;
using Store.Models.Enums;
using StoreUI.Services;

namespace StoreUI.Pages;

public class PaymentsModel : SecurePageModel
{
    private readonly IApiClientService _apiClient;

    public IReadOnlyList<MobileMoneyTransactionDto> Transactions { get; private set; } = Array.Empty<MobileMoneyTransactionDto>();
    public SettlementReportDto? Settlement { get; private set; }
    public int CurrentPage { get; private set; } = 1;
    public int PageSize { get; private set; } = 50;
    public MobileMoneyStatus? StatusFilter { get; private set; }
    public DateTime? FromDate { get; private set; }
    public DateTime? ToDate { get; private set; }

    [TempData] public string? StatusMessage { get; set; }

    public PaymentsModel(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(
        int page = 1,
        MobileMoneyStatus? status = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return GoToLogin();

        if (!HasPermission(permissions, PermissionKeys.PaymentsRead))
            return AccessDenied();

        _apiClient.SetToken(token);
        CurrentPage = Math.Max(1, page);
        StatusFilter = status;
        FromDate = from ?? DateTime.UtcNow.Date;
        ToDate = to ?? DateTime.UtcNow.Date;

        var statusParam = status.HasValue ? $"&status={(int)status.Value}" : string.Empty;
        var txUrl = $"/api/payments/momo?page={CurrentPage}&pageSize={PageSize}{statusParam}";
        var settlementUrl = $"/api/payments/settlement?fromDate={FromDate:yyyy-MM-dd}&toDate={ToDate:yyyy-MM-dd}";

        var txTask = _apiClient.GetAsync<List<MobileMoneyTransactionDto>>(txUrl, ct);
        var settlementTask = _apiClient.GetAsync<SettlementReportDto>(settlementUrl, ct);

        await Task.WhenAll(txTask, settlementTask);

        var txList = await txTask;
        Transactions = (IReadOnlyList<MobileMoneyTransactionDto>?)txList?.AsReadOnly() ?? Array.Empty<MobileMoneyTransactionDto>();
        Settlement = await settlementTask;

        return Page();
    }
}

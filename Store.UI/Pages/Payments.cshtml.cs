using Microsoft.AspNetCore.Mvc;
using Store.Models.DTOs.Operations;
using Store.Models.DTOs.Payments;
using Store.Models.Enums;
using StoreUI.Services;

namespace StoreUI.Pages;

public class PaymentsModel : SecurePageModel
{
    private readonly IPaymentsManager _paymentsManager;
    private readonly IApiClientService _apiClient;

    public IReadOnlyList<MobileMoneyTransactionDto> Transactions { get; private set; } = Array.Empty<MobileMoneyTransactionDto>();
    public SettlementReportDto? Settlement { get; private set; }
    public int CurrentPage { get; private set; } = 1;
    public int PageSize { get; private set; } = 50;
    public MobileMoneyStatus? StatusFilter { get; private set; }
    public DateTime? FromDate { get; private set; }
    public DateTime? ToDate { get; private set; }

    [TempData] public string? StatusMessage { get; set; }

    public PaymentsModel(IPaymentsManager paymentsManager, IApiClientService apiClient)
    {
        _paymentsManager = paymentsManager;
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

        var txTask = _paymentsManager.GetTransactionsAsync(CurrentPage, PageSize, StatusFilter, ct);
        var settlementTask = _paymentsManager.GetSettlementReportAsync(FromDate.Value, ToDate.Value, ct);

        await Task.WhenAll(txTask, settlementTask);

        var txList = await txTask;
        Transactions = txList?.AsReadOnly() ?? (IReadOnlyList<MobileMoneyTransactionDto>)Array.Empty<MobileMoneyTransactionDto>();
        Settlement = await settlementTask;

        return Page();
    }

    public async Task<IActionResult> OnGetQueryStatusAsync(Guid id, CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return new JsonResult(new { success = false, message = "Authentication required." }) { StatusCode = 401 };

        if (!HasPermission(permissions, PermissionKeys.PaymentsRead))
            return StatusCode(403, new { success = false, message = "Access denied: PaymentsRead permission required." });

        _apiClient.SetToken(token);

        if (id == Guid.Empty)
            return BadRequest(new { success = false, message = "Valid Transaction ID is required." });

        var tx = await _paymentsManager.QueryTransactionStatusAsync(id, ct);
        if (tx is null)
            return NotFound(new { success = false, message = "Transaction not found on gateway." });

        return new JsonResult(new
        {
            success = true,
            transactionId = tx.MobileMoneyTransactionId,
            status = tx.Status,
            isPending = string.Equals(tx.Status, "Pending", StringComparison.OrdinalIgnoreCase),
            isSuccess = string.Equals(tx.Status, "Completed", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(tx.Status, "Successful", StringComparison.OrdinalIgnoreCase),
            completedAt = tx.CompletedAtUtc?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Pending",
            providerRef = tx.ProviderTransactionId ?? "N/A",
            message = $"Transaction #{tx.MobileMoneyTransactionId.ToString()[..8]} status: {tx.Status}"
        });
    }

    public async Task<IActionResult> OnGetExportCsvAsync(
        MobileMoneyStatus? status = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken ct = default)
    {
        if (!TryGetSecurityContext(out var token, out var permissions))
            return Unauthorized();

        if (!HasPermission(permissions, PermissionKeys.PaymentsRead))
            return Forbid();

        _apiClient.SetToken(token);

        var fromDt = from ?? DateTime.UtcNow.Date;
        var toDt = to ?? DateTime.UtcNow.Date;

        var txTask = _paymentsManager.GetTransactionsAsync(1, 1000, status, ct);
        var settlementTask = _paymentsManager.GetSettlementReportAsync(fromDt, toDt, ct);

        await Task.WhenAll(txTask, settlementTask);

        var transactions = await txTask;
        var settlement = await settlementTask;

        var csvBytes = _paymentsManager.GenerateSettlementCsv(settlement, transactions);
        var filename = $"electronic_settlement_{fromDt:yyyyMMdd}_{toDt:yyyyMMdd}.csv";

        return File(csvBytes, "text/csv", filename);
    }
}

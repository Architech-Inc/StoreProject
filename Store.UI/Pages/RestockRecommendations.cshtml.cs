using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;
using StoreUI.Services;

namespace StoreUI.Pages;

public class RestockRecommendationsModel : SecurePageModel
{
    private readonly IApiClientService _apiClient;
    private readonly ILogger<RestockRecommendationsModel> _logger;

    public RestockRecommendationsModel(IApiClientService apiClient, ILogger<RestockRecommendationsModel> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public List<RestockRecommendationDto> Recommendations { get; set; } = new();
    public RestockSummaryDto? Summary { get; set; }
    public BulkOrderResultDto? LastBulkResult { get; set; }

    public string? StatusMessage { get; set; }
    public bool StatusIsError { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var token = HttpContext.Session.GetString("access_token");
        if (string.IsNullOrWhiteSpace(token))
        {
            return GoToLogin();
        }
        _apiClient.SetToken(token);

        try
        {
            var recsTask = _apiClient.GetAsync<List<RestockRecommendationDto>>("api/Restock/pending");
            var summaryTask = _apiClient.GetAsync<RestockSummaryDto>("api/Restock/summary");
            await Task.WhenAll(recsTask, summaryTask);

            if (recsTask.Result is not null) Recommendations = recsTask.Result;
            Summary = summaryTask.Result;

            if (TempData.TryGetValue("BulkOrderResult", out var raw) && raw is string json)
            {
                try
                {
                    LastBulkResult = System.Text.Json.JsonSerializer.Deserialize<BulkOrderResultDto>(json);
                }
                catch (Exception parseEx)
                {
                    _logger.LogWarning(parseEx, "Failed to deserialize stored bulk-order result.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load restock recommendations.");
            StatusMessage = "Failed to load restock recommendations.";
            StatusIsError = true;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostConvertToTransferAsync(Guid id)
    {
        var token = HttpContext.Session.GetString("access_token");
        if (string.IsNullOrWhiteSpace(token)) return GoToLogin();
        _apiClient.SetToken(token);
        try
        {
            var result = await _apiClient.PostAsync<RestockConversionResultDto>($"api/Restock/{id}/convert-to-transfer", null);
            StatusMessage = result?.Message ?? "Transfer request submitted.";
            StatusIsError = !(result?.Success ?? false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert recommendation {Id} to transfer.", id);
            StatusMessage = "Failed to submit transfer request.";
            StatusIsError = true;
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostConvertToPurchaseOrderAsync(Guid id)
    {
        var token = HttpContext.Session.GetString("access_token");
        if (string.IsNullOrWhiteSpace(token)) return GoToLogin();
        _apiClient.SetToken(token);
        try
        {
            var result = await _apiClient.PostAsync<RestockConversionResultDto>($"api/Restock/{id}/convert-to-po", null);
            StatusMessage = result?.Message ?? "Purchase order submitted.";
            StatusIsError = !(result?.Success ?? false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert recommendation {Id} to PO.", id);
            StatusMessage = "Failed to submit purchase order.";
            StatusIsError = true;
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDismissAsync(Guid id)
    {
        var token = HttpContext.Session.GetString("access_token");
        if (string.IsNullOrWhiteSpace(token)) return GoToLogin();
        _apiClient.SetToken(token);
        try
        {
            var result = await _apiClient.PostAsync<RestockConversionResultDto>($"api/Restock/{id}/dismiss", null);
            StatusMessage = result?.Message ?? "Recommendation dismissed.";
            StatusIsError = !(result?.Success ?? false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dismiss recommendation {Id}.", id);
            StatusMessage = "Failed to dismiss recommendation.";
            StatusIsError = true;
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostBulkOrderCriticalAsync()
    {
        var token = HttpContext.Session.GetString("access_token");
        if (string.IsNullOrWhiteSpace(token)) return GoToLogin();
        _apiClient.SetToken(token);
        try
        {
            var result = await _apiClient.PostAsync<BulkOrderResultDto>("api/Restock/bulk-order-critical", null);
            if (result is null)
            {
                StatusMessage = "Bulk-order failed: server returned no result.";
                StatusIsError = true;
            }
            else
            {
                LastBulkResult = result;
                TempData["BulkOrderResult"] = System.Text.Json.JsonSerializer.Serialize(result);
                StatusMessage = result.Success
                    ? $"{result.PurchaseOrdersCreated} purchase order(s) created; {result.Skipped} skipped."
                    : result.Message;
                StatusIsError = !result.Success;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk-order critical failed.");
            StatusMessage = "Bulk-order failed; see server logs for details.";
            StatusIsError = true;
        }
        return RedirectToPage();
    }
}

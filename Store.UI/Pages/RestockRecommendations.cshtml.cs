using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.Models.Entities.Inventory;
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

    public List<RestockRecommendation> Recommendations { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        _apiClient.SetToken(HttpContext.Session.GetString("access_token"));
        
        try
        {
            var result = await _apiClient.GetAsync<List<RestockRecommendation>>("api/Restock/pending");
            if (result != null)
            {
                Recommendations = result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load restock recommendations.");
            TempData["StatusMessage"] = "Error: Failed to load recommendations.";
        }
        
        return Page();
    }

    public async Task<IActionResult> OnPostConvertToTransferAsync(Guid id)
    {
        _apiClient.SetToken(HttpContext.Session.GetString("access_token"));
        var success = await _apiClient.PostAsync($"api/Restock/{id}/convert-to-transfer", null);
        if (success)
        {
            TempData["StatusMessage"] = "Successfully generated stock transfer request.";
        }
        else
        {
            TempData["StatusMessage"] = "Error: Failed to generate stock transfer.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostConvertToPurchaseOrderAsync(Guid id)
    {
        _apiClient.SetToken(HttpContext.Session.GetString("access_token"));
        var success = await _apiClient.PostAsync($"api/Restock/{id}/convert-to-po", null);
        if (success)
        {
            TempData["StatusMessage"] = "Successfully generated purchase order.";
        }
        else
        {
            TempData["StatusMessage"] = "Error: Failed to generate purchase order.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDismissAsync(Guid id)
    {
        _apiClient.SetToken(HttpContext.Session.GetString("access_token"));
        var success = await _apiClient.PostAsync($"api/Restock/{id}/dismiss", null);
        if (success)
        {
            TempData["StatusMessage"] = "Recommendation dismissed.";
        }
        else
        {
            TempData["StatusMessage"] = "Error: Failed to dismiss recommendation.";
        }
        return RedirectToPage();
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StoreUI.Services;

namespace StoreUI.Pages;

public class LogoutModel : PageModel
{
    private readonly IApiClientService _apiClient;

    public LogoutModel(IApiClientService apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        await PerformLogoutAsync(ct);
        return RedirectToPage("/Login");
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        await PerformLogoutAsync(ct);
        return RedirectToPage("/Login");
    }

    private async Task PerformLogoutAsync(CancellationToken ct)
    {
        var token = HttpContext.Session.GetString("access_token");
        if (!string.IsNullOrWhiteSpace(token))
        {
            try
            {
                _apiClient.SetToken(token);
                await _apiClient.PostAsync<object>("/api/auth/logout", new { }, ct);
            }
            catch
            {
                // Silently ignore network failures on logout so session is always cleared locally
            }
        }

        HttpContext.Session.Remove("access_token");
        HttpContext.Session.Remove("refresh_token");
        HttpContext.Session.Clear();
    }
}

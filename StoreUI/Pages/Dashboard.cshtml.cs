using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StoreUI.Pages;

public class DashboardModel : PageModel
{
    public void OnGet()
    {
        if (!User?.Identity?.IsAuthenticated ?? true)
            RedirectToPage("Login");
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StoreUI.Pages;

public class DashboardModel : PageModel
{
    public IActionResult OnGet()
    {
        if (!User?.Identity?.IsAuthenticated ?? true)
            return RedirectToPage("Login");

        return Page();
    }
}

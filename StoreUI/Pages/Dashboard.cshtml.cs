using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StoreUI.Pages;

public class DashboardModel : PageModel
{
    public IActionResult OnGet()
    {
        var token = HttpContext.Session.GetString("access_token");
        if (string.IsNullOrEmpty(token))
            return RedirectToPage("Login");

        return Page();
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Store.UI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // If the user is authenticated, we could theoretically redirect them directly to the Dashboard.
            // However, a landing page is useful for logging out / returning to the root.
            // We'll leave it as a standard page load and the UI handles the state.
            return Page();
        }
    }
}

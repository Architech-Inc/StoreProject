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

        public bool IsAuthenticated { get; set; }

        public IActionResult OnGet()
        {
            var token = HttpContext.Session.GetString("access_token");
            IsAuthenticated = !string.IsNullOrEmpty(token);
            
            return Page();
        }
    }
}

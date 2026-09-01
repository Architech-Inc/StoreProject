using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages;

public class IndexModel : PageModel
{
    private readonly IPortalSessionService _sessionService;

    public IndexModel(IPortalSessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public bool IsAuthenticated => Session != null;
    public PortalSession? Session { get; set; }

    public void OnGet()
    {
        Session = _sessionService.GetCurrentSession(User);
    }
}

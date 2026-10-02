using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.TenantPortal.Models.ViewModels;
using Store.TenantPortal.Services;

using Store.Models.Common;
namespace Store.TenantPortal.Pages;

public class RegisterModel : PageModel
{
    private readonly ILogger<RegisterModel> _logger;
    private readonly IControlPlaneClient _cpClient;
    private readonly IPortalSessionService _sessionService;

    public RegisterModel(IControlPlaneClient cpClient, IPortalSessionService sessionService, ILogger<RegisterModel> logger)
    {
        _cpClient = cpClient;
        _sessionService = sessionService;
    
        _logger = logger;}

    [BindProperty]
    public RegisterVm Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Onboarding");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var authResult = await _cpClient.RegisterAccountAsync(Input.Email, Input.FullName, Input.Password, ct);
            await _sessionService.SignInAsync(HttpContext, authResult);

            return RedirectToPage("/Onboarding");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to ControlPlane service");
            ErrorMessage = "Unable to connect to the Control Plane management service. Please ensure Store.ControlPlane is running on port 19999.";
            return Page();
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = SafeErrorMessage.From(ex, _logger, "Register operation");
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration");
            ErrorMessage = "An unexpected error occurred during registration. Please try again.";
            return Page();
        }
    }
}

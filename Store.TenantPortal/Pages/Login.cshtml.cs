using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.Models.Common;
using Store.TenantPortal.Models.ViewModels;
using Store.TenantPortal.Services;

namespace Store.TenantPortal.Pages;

public class LoginModel : PageModel
{
    private readonly IControlPlaneClient _cpClient;
    private readonly IPortalSessionService _sessionService;
    private readonly ILogger<LoginModel>? _logger;

    public LoginModel(
        IControlPlaneClient cpClient,
        IPortalSessionService sessionService,
        ILogger<LoginModel>? logger = null)
    {
        _cpClient = cpClient;
        _sessionService = sessionService;
        _logger = logger;
    }

    [BindProperty]
    public LoginVm Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var session = _sessionService.GetCurrentSession(User);
            if (session?.HasTenant == true)
            {
                return RedirectToPage("/Dashboard");
            }
            return RedirectToPage("/Onboarding");
        }

        Input.ReturnUrl = returnUrl;
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
            var authResult = await _cpClient.LoginAsync(Input.Email, Input.Password, ct);
            if (authResult == null)
            {
                ErrorMessage = "Invalid email address or password.";
                return Page();
            }

            await _sessionService.SignInAsync(HttpContext, authResult);

            if (!string.IsNullOrEmpty(Input.ReturnUrl) && Url.IsLocalUrl(Input.ReturnUrl))
            {
                return Redirect(Input.ReturnUrl);
            }

            if (authResult.TenantId.HasValue && !string.IsNullOrEmpty(authResult.TenantSlug))
            {
                return RedirectToPage("/Dashboard");
            }

            return RedirectToPage("/Onboarding");
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Failed to connect to ControlPlane service");
            ErrorMessage = "Unable to connect to the Control Plane management service. Please ensure Store.ControlPlane is running on port 19999.";
            return Page();
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = SafeErrorMessage.From(ex, _logger, "Login operation");
            return Page();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error during login");
            ErrorMessage = "An unexpected error occurred during login. Please try again.";
            return Page();
        }
    }
}

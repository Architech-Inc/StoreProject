using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.Models.DTOs.Auth;
using Store.Models.Interfaces.Services;

namespace StoreUI.Pages;

public class LoginModel : PageModel
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<LoginModel> _logger;

    [BindProperty]
    public string? Username { get; set; }

    [BindProperty]
    public string? Password { get; set; }

    public string? ErrorMessage { get; set; }

    public LoginModel(IAuthenticationService authService, ILogger<LoginModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        if (User?.Identity?.IsAuthenticated == true)
            return RedirectToPage("Dashboard");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username and password are required.";
            return Page();
        }

        var request = new LoginRequest { Username = Username, Password = Password };
        var response = await _authService.LoginAsync(request);

        if (response is null)
        {
            ErrorMessage = "Login failed. Please check your credentials.";
            _logger.LogWarning("Failed login attempt for user: {Username}", Username);
            return Page();
        }

        // Store token in session
        HttpContext.Session.SetString("access_token", response.AccessToken);
        if (!string.IsNullOrEmpty(response.RefreshToken))
            HttpContext.Session.SetString("refresh_token", response.RefreshToken);

        _logger.LogInformation("User {Username} logged in successfully", Username);
        return RedirectToPage("Dashboard");
    }
}

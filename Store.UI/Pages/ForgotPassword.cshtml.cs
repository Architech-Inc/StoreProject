using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.Models.DTOs.Auth;

namespace StoreUI.Pages;

public class ForgotPasswordModel : PageModel
{
    private readonly ILogger<ForgotPasswordModel> _logger;
    private readonly HttpClient _httpClient;

    public ForgotPasswordModel(ILogger<ForgotPasswordModel> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("StoreApi");
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var request = new ForgotPasswordRequest { UsernameOrEmail = Input.Email };
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/auth/forgot-password", content);

            // Regardless of response, we show a success message to prevent user enumeration
            TempData["SuccessMessage"] = "If an account with that email exists, a password reset link has been sent.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during forgot password request.");
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
            return Page();
        }
    }
}

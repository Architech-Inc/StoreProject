using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.Models.DTOs.Auth;

namespace StoreUI.Pages;

public class ResetPasswordModel : PageModel
{
    private readonly ILogger<ResetPasswordModel> _logger;
    private readonly HttpClient _httpClient;

    public ResetPasswordModel(ILogger<ResetPasswordModel> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("StoreApi");
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool IsError { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public class InputModel
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(128, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public IActionResult OnGet(string token, string email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        {
            IsError = true;
            ErrorMessage = "Invalid password reset link.";
            return Page();
        }

        Input = new InputModel
        {
            Token = token,
            Email = email
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var request = new ConfirmPasswordResetRequest
            {
                Token = Input.Token,
                Email = Input.Email,
                NewPassword = Input.NewPassword,
                ConfirmPassword = Input.ConfirmPassword
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/auth/reset-password-confirm", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Your password has been successfully reset.";
                return RedirectToPage();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Reset password failed. Status Code: {StatusCode}. Response: {Response}", response.StatusCode, errorContent);
            
            IsError = true;
            ErrorMessage = "Your password reset link is invalid or has expired.";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during password reset confirmation.");
            IsError = true;
            ErrorMessage = "An unexpected error occurred. Please try again later.";
            return Page();
        }
    }
}

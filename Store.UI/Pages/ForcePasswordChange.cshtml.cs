using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Store.Models.DTOs.Auth;

namespace StoreUI.Pages;

public class ForcePasswordChangeModel : PageModel
{
    private readonly ILogger<ForcePasswordChangeModel> _logger;
    private readonly HttpClient _httpClient;

    public ForcePasswordChangeModel(ILogger<ForcePasswordChangeModel> logger, IHttpClientFactory httpClientFactory)
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
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(128, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public IActionResult OnGet()
    {
        var token = HttpContext.Session.GetString("access_token");
        if (string.IsNullOrEmpty(token))
            return RedirectToPage("/Login");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var token = HttpContext.Session.GetString("access_token");
        if (string.IsNullOrEmpty(token))
            return RedirectToPage("/Login");

        var username = HttpContext.Session.GetString("username");
        if (string.IsNullOrEmpty(username))
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }

        try
        {
            var request = new ResetPasswordRequest
            {
                Username = username,
                CurrentPassword = Input.CurrentPassword,
                NewPassword = Input.NewPassword,
                ConfirmPassword = Input.ConfirmPassword
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.PostAsync("/api/auth/reset-password", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["ToastSuccess"] = "Your password has been successfully updated.";
                return RedirectToPage("/Dashboard");
            }

            IsError = true;
            ErrorMessage = "Failed to update password. Please ensure your current password is correct.";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during forced password update.");
            IsError = true;
            ErrorMessage = "An unexpected error occurred. Please try again later.";
            return Page();
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Store.Models.DTOs.Auth;

public class LoginRequest
{
    [Required, StringLength(100, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class LoginWithEmailRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class LoginWithPhoneRequest
{
    [Required]
    public int CountryId { get; set; }

    [Required, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}

public class RegisterRequest
{
    [Required, StringLength(100, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    public int RoleId { get; set; } = 1;
}

public class ResetPasswordRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    /// <summary>
    /// The expired access token. Still useful for audit logging which user
    /// is asking for the refresh, but not strictly required.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// SEC-28 — refresh token. Now optional in the JSON body — when the
    /// caller doesn't supply one, the controller reads the <c>store_rt</c>
    /// HttpOnly cookie. JSON-body transmission is retained for non-browser
    /// callers (CLI tools, Postman) but browser clients should send the
    /// cookie automatically.
    /// </summary>
    public string? RefreshToken { get; set; }
}

public class RequestOtpRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;
}

public class VerifyOtpRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;
    [Required]
    public string OtpCode { get; set; } = string.Empty;
}

public class RecoverPasswordWithTokenRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace Store.Models.DTOs.Auth;

public class Login2FARequest
{
    [Required]
    public string TwoFactorToken { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;
}

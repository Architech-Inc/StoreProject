using System.ComponentModel.DataAnnotations;
using Store.Models.Enums;

namespace Store.Models.DTOs.Users;

public class UserDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string? RoleName { get; set; }
    public Guid? EmployeeId { get; set; }
    public UserStatus Status { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? FullImageUrl { get; set; }
    public DateTime DateCreated { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? PrimaryPhone { get; set; }
    public bool TwoFactorEnabled { get; set; }
}

public class CreateUserRequest
{
    [Required, StringLength(100, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    public int RoleId { get; set; } = 1;
    public Guid? EmployeeId { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? FullImageUrl { get; set; }
}

public class UpdateUserRequest
{
    [StringLength(100, MinimumLength = 3)]
    public string? Username { get; set; }

    public int? RoleId { get; set; }
    public UserStatus? Status { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? FullImageUrl { get; set; }
}

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class UpdateUserContactsRequest
{
    [EmailAddress, StringLength(254)]
    public string? Email { get; set; }

    [Phone, StringLength(50)]
    public string? Phone { get; set; }
}

public class Enable2FAResponse
{
    public string SharedKey { get; set; } = string.Empty;
    public string AuthenticatorUri { get; set; } = string.Empty;
}

public class Verify2FARequest
{
    [Required, StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
}

public class AuditLogDto
{
    public long Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime DateCreated { get; set; }
}

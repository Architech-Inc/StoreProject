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
    public string? ImagePath { get; set; }
    public DateTime DateCreated { get; set; }
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
}

public class UpdateUserRequest
{
    [StringLength(100, MinimumLength = 3)]
    public string? Username { get; set; }

    public int? RoleId { get; set; }
    public UserStatus? Status { get; set; }
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

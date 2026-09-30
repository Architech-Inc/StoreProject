using System.ComponentModel.DataAnnotations;

namespace Store.Models.DTOs.Operations;

public class RolePermissionDto
{
    public long RolePermissionId { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string PermissionKey { get; set; } = string.Empty;
    public bool IsAllowed { get; set; }
}

public class RoleMatrixDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public Dictionary<string, bool> Permissions { get; set; } = new();
}

public class UpdateRolePermissionRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "RoleId must be greater than 0.")]
    public int RoleId { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "PermissionKey is required.")]
    [StringLength(120, MinimumLength = 1)]
    public string PermissionKey { get; set; } = string.Empty;

    public bool IsAllowed { get; set; }
}

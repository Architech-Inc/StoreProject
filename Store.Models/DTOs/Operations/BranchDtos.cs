using System.ComponentModel.DataAnnotations;

namespace Store.Models.DTOs.Operations;

public class BranchDto
{
    public int BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; }
}

public class UpsertBranchRequest
{
    public int? BranchId { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;
}

public class UserBranchRoleDto
{
    public long UserBranchRoleId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}

public class AssignUserBranchRoleRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public int BranchId { get; set; }

    [Required]
    public int RoleId { get; set; }
}

public class RemoveUserBranchRoleRequest
{
    [Required]
    public long UserBranchRoleId { get; set; }
}

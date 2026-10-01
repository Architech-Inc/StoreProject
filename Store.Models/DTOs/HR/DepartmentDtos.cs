using System.ComponentModel.DataAnnotations;

namespace Store.Models.DTOs.HR;

public record CreateDepartmentRequest(
    [Required, StringLength(150, MinimumLength = 1)] string Name,
    [StringLength(500)] string? Description = null);

public record UpdateDepartmentRequest(
    [Required, StringLength(150, MinimumLength = 1)] string Name,
    [StringLength(500)] string? Description = null);

/// <summary>
/// Alias kept for backward compatibility with existing callers.
/// </summary>
public record CreateLookupRequest(
    [Required, StringLength(150, MinimumLength = 1)] string Name,
    [StringLength(500)] string? Description = null);

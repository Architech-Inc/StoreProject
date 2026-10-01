using System.ComponentModel.DataAnnotations;

namespace Store.Models.DTOs.Items;

public record CreateCategoryRequest(
    [Required, StringLength(150, MinimumLength = 1)] string Name,
    [StringLength(500)] string? Description = null,
    [StringLength(500)] string? ThumbnailUrl = null,
    [StringLength(500)] string? FullImageUrl = null);

public record UpdateCategoryRequest(
    [Required, StringLength(150, MinimumLength = 1)] string Name,
    [StringLength(500)] string? Description = null,
    [StringLength(500)] string? ThumbnailUrl = null,
    [StringLength(500)] string? FullImageUrl = null);

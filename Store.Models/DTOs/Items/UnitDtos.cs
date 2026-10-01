using System.ComponentModel.DataAnnotations;

namespace Store.Models.DTOs.Items;

public record CreateUnitRequest(
    [Required, StringLength(100, MinimumLength = 1)] string Name,
    [Required, StringLength(20, MinimumLength = 1)] string Abbreviation,
    [StringLength(300)] string? Description = null);

public record UpdateUnitRequest(
    [Required, StringLength(100, MinimumLength = 1)] string Name,
    [Required, StringLength(20, MinimumLength = 1)] string Abbreviation,
    [StringLength(300)] string? Description = null);

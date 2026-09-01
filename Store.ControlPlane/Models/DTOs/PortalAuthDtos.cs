using System.ComponentModel.DataAnnotations;

namespace Store.ControlPlane.Models.DTOs;

public record RegisterPortalAccountRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(2)] string FullName,
    [Required, MinLength(8)] string Password
);

public record LoginPortalAccountRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record PortalAuthResponse(
    Guid AccountId,
    string Email,
    string FullName,
    Guid? TenantId,
    string? TenantSlug,
    string? TenantName,
    string SessionToken,
    DateTime ExpiresAt
);

public record SlugCheckResponse(
    string Slug,
    bool IsAvailable,
    string? Reason = null
);

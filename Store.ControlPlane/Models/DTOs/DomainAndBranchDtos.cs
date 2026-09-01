using System.ComponentModel.DataAnnotations;

namespace Store.ControlPlane.Models.DTOs;

public record TenantDomainDto(
    Guid TenantId,
    string Slug,
    string PlatformUiUrl,
    string PlatformApiUrl,
    string CustomDomain,
    string CustomDomainStatus,
    string VerificationRecordName,
    string VerificationRecordValue,
    DateTime? CustomDomainVerifiedAt,
    string? LastErrorMessage
);

public record SetCustomDomainRequest(
    [Required, RegularExpression(@"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$", ErrorMessage = "Invalid domain format.")]
    string Domain
);

public record VerifyDomainResponse(
    string Domain,
    bool IsVerified,
    string Status,
    string? CheckedHost,
    string? ExpectedValue,
    List<string>? FoundValues,
    string? Message
);

public record BranchDto(
    Guid BranchId,
    string BranchName,
    string BranchSlug,
    string DomainType,
    string? CustomSubdomain,
    string ResolvedUrl,
    string VerificationStatus,
    string VerificationRecordName,
    string VerificationRecordValue,
    DateTime DateCreated
);

public record CreateBranchRequest(
    [Required, MinLength(2), MaxLength(50)] string BranchName,
    [Required, RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Branch slug can only contain lowercase letters, numbers, and hyphens.")] string BranchSlug,
    string DomainType = "Platform", // "Platform" or "Custom"
    string? CustomSubdomain = null
);

public record EnvironmentStatusDto(
    Guid TenantId,
    string TenantName,
    string Slug,
    string Status,
    bool IsHealthy,
    DateTime? LastHealthCheck,
    string? LastHealthMessage,
    List<ContainerStatusDto> Containers
);

public record ContainerStatusDto(
    string Name,
    string ContainerName,
    string ServiceType,
    string Image,
    string Status,
    bool IsHealthy,
    DateTime? LastChecked
);

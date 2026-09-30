namespace Store.Models.Billing;

/// <summary>Validation failure detail (serialized to HTTP 422 body).</summary>
public sealed class PosOverrideValidationFailure
{
    public required string? ClientSessionId { get; init; }
    public required string CartFingerprint { get; init; }
    public required Guid ActingUserId { get; init; }
    public List<string> Reasons { get; } = new();
}

namespace Store.Models.DTOs.Auth;

/// <summary>
/// SEC-23 — public-facing view of a <see cref="Entities.TrustedDevice"/>.
/// Never exposes the server-only fingerprint hash or the raw IP. The
/// public <see cref="DeviceId"/> is the opaque token the UI uses to
/// revoke / trust a device — it is collision-safe (24 random bytes,
/// URL-safe base64) but otherwise meaningless to the client.
/// </summary>
public class TrustedDeviceDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public DateTime FirstSeenAtUtc { get; set; }
    public DateTime LastSeenAtUtc { get; set; }
    public DateTime? LastWebAuthnAtUtc { get; set; }
    public bool IsTrusted { get; set; }
    public DateTime? TrustedUntilUtc { get; set; }

    /// <summary>True when this row corresponds to the current request's device.</summary>
    public bool IsCurrentDevice { get; set; }
}

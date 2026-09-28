using Store.Models.Entities.Base;

namespace Store.Models.Entities;

/// <summary>
/// SEC-23 — WebAuthn enrolled-device binding.
///
/// One row per (user, device) pair. The <see cref="FingerprintHash"/>
/// is the canonical identity of the device — it derives from the request
/// User-Agent + IP /24 (or /48 for IPv6) + Accept-Language — and is used
/// to detect when a refresh token is replayed from a new / unknown device.
///
/// Trust states:
///   - <see cref="IsRevoked"/> = true → soft-deleted. Any login attempt
///     from this fingerprint is treated as a brand-new device.
///   - <see cref="IsTrusted"/> = true → operator-promoted to skip IP
///     anomaly checks for the configured window (default 30 days, see
///     <see cref="TrustedUntilUtc"/>). Used for shared office devices
///     where the IP may rotate.
/// </summary>
public class TrustedDevice : BaseEntity
{
    public int TrustedDeviceId { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// Public opaque device id surfaced to clients (32-char URL-safe base64).
    /// Distinct from the fingerprint hash so the fingerprint stays server-only.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 hex of (User-Agent + normalized IP CIDR + Accept-Language).
    /// Stable per-device across sessions; different IPs within the same
    /// /24 (IPv4) or /48 (IPv6) collapse to the same fingerprint so
    /// mobile-network IP rotation does not register as a new device.
    /// </summary>
    public string FingerprintHash { get; set; } = string.Empty;

    public string DeviceName { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public string? IpAddressCidr { get; set; }

    public DateTime FirstSeenAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>When WebAuthn was last used on this device (null = never).</summary>
    public DateTime? LastWebAuthnAtUtc { get; set; }

    public bool IsRevoked { get; set; }

    /// <summary>Operator-promoted to skip IP anomaly checks until <see cref="TrustedUntilUtc"/>.</summary>
    public bool IsTrusted { get; set; }

    public DateTime? TrustedUntilUtc { get; set; }

    public User User { get; set; } = null!;
}

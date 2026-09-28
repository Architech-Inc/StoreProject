using Microsoft.AspNetCore.Http;
using Store.Models.Entities;

namespace Store.Models.Interfaces.Services;

/// <summary>
/// SEC-23 — service surface that orchestrates <see cref="TrustedDevice"/>
/// registration + lookup from request context. Stores the device
/// fingerprint hash (server-only) and the public device id (client-safe).
///
/// Lifecycle:
///   1. <see cref="RegisterOrUpdateAsync"/> — called from every login /
///      refresh path. If the fingerprint matches a known device, updates
///      LastSeenAtUtc + DeviceName. Otherwise creates a new row.
///   2. <see cref="FindByFingerprintAsync"/> — called from the refresh
///      path to detect whether a token is being replayed from a new
///      device (and therefore needs step-up WebAuthn).
///   3. <see cref="MarkWebAuthnUsedAsync"/> — called after a successful
///      /webauthn/makeAssertion. Stamps LastWebAuthnAtUtc so the
///      anomaly detector can distinguish "enrolled device" from
///      "first-time WebAuthn from this device".
/// </summary>
public interface ITrustedDeviceService
{
    /// <summary>
    /// Idempotent register-or-update. Reads User-Agent + IP +
    /// Accept-Language from <paramref name="httpContext"/>, computes the
    /// fingerprint hash, and either updates the existing device row or
    /// inserts a new one. Returns the resulting entity (including the
    /// newly-issued public DeviceId when this is a first-seen device).
    /// </summary>
    Task<TrustedDevice> RegisterOrUpdateAsync(Guid userId, HttpContext httpContext, CancellationToken ct = default);

    /// <summary>Look up a device by fingerprint (used by the refresh path).</summary>
    Task<TrustedDevice?> FindByFingerprintAsync(Guid userId, string fingerprintHash, CancellationToken ct = default);

    /// <summary>
    /// Bump <c>LastSeenAtUtc</c> on the device row. Optionally stamp
    /// <c>LastWebAuthnAtUtc</c> when WebAuthn was the auth method.
    /// </summary>
    Task MarkUsedAsync(int trustedDeviceId, bool usedWebAuthn, CancellationToken ct = default);

    /// <summary>List every non-revoked device the user has enrolled.</summary>
    Task<IReadOnlyList<TrustedDevice>> ListAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Soft-revoke a device. Returns false if the row does not exist or does not belong to the user.</summary>
    Task<bool> RevokeAsync(Guid userId, string deviceId, CancellationToken ct = default);

    /// <summary>Promote a device to operator-trusted for a 30-day window (skip IP anomaly on refresh).</summary>
    Task<bool> TrustAsync(Guid userId, string deviceId, TimeSpan window, CancellationToken ct = default);
}

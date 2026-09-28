using Store.Models.Entities;

namespace Store.Models.Interfaces.Services;

/// <summary>
/// SEC-23 — repository abstraction over the <c>trusted_devices</c> table.
/// Mirrors the <c>ITenantRepository</c> pattern so tests can mock the
/// storage layer without spinning up a real DbContext.
/// </summary>
public interface ITrustedDeviceRepository
{
    /// <summary>Returns the device matching the (userId, fingerprintHash) tuple, or null.</summary>
    Task<TrustedDevice?> FindByFingerprintAsync(Guid userId, string fingerprintHash, CancellationToken ct = default);

    /// <summary>Returns every non-revoked device the user has registered, newest first.</summary>
    Task<IReadOnlyList<TrustedDevice>> ListAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns the device by its public id, scoped to the user (defense-in-depth on revoke).</summary>
    Task<TrustedDevice?> FindByPublicIdAsync(Guid userId, string deviceId, CancellationToken ct = default);

    /// <summary>Insert or update — by fingerprint when present, otherwise append a new row.</summary>
    Task<TrustedDevice> UpsertAsync(TrustedDevice device, CancellationToken ct = default);

    /// <summary>Find by primary key (used by the WebAuthn stamp path).</summary>
    Task<TrustedDevice?> FindByIdAsync(int trustedDeviceId, CancellationToken ct = default);

    /// <summary>Mark the row as revoked (soft delete). Idempotent.</summary>
    Task<bool> RevokeAsync(Guid userId, string deviceId, CancellationToken ct = default);

    /// <summary>Promote a device to operator-trusted until <paramref name="untilUtc"/>.</summary>
    Task<bool> SetTrustedAsync(Guid userId, string deviceId, DateTime untilUtc, CancellationToken ct = default);
}

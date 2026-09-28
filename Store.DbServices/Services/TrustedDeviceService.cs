using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;
using Store.Models.Security;

namespace Store.DbServices.Services;

/// <summary>
/// SEC-23 — orchestrates <see cref="TrustedDevice"/> registration +
/// lookup from <see cref="HttpContext"/>. Reads User-Agent / IP /
/// Accept-Language from the request, computes the fingerprint hash, and
/// defers persistence to <see cref="ITrustedDeviceRepository"/>.
///
/// All randomness here comes from <see cref="RandomNumberGenerator"/> —
/// no <c>Random</c>, no predictable seeds.
/// </summary>
public class TrustedDeviceService : ITrustedDeviceService
{
    private static readonly TimeSpan DefaultTrustWindow = TimeSpan.FromDays(30);

    private readonly ITrustedDeviceRepository _repo;
    private readonly TimeProvider _clock;

    public TrustedDeviceService(ITrustedDeviceRepository repo, TimeProvider? clock = null)
    {
        _repo = repo;
        _clock = clock ?? TimeProvider.System;
    }

    public async Task<TrustedDevice> RegisterOrUpdateAsync(Guid userId, HttpContext httpContext, CancellationToken ct = default)
    {
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var acceptLanguage = httpContext.Request.Headers.AcceptLanguage.ToString();

        var fingerprint = DeviceFingerprint.ComputeHash(userAgent, ipAddress, acceptLanguage);
        var existing = await _repo.FindByFingerprintAsync(userId, fingerprint, ct);

        if (existing is not null)
        {
            existing.LastSeenAtUtc = _clock.GetUtcNow().UtcDateTime;
            existing.DeviceName = DeviceFingerprint.DeriveName(userAgent);
            existing.UserAgent = userAgent.Length > 500 ? userAgent[..500] : userAgent;
            existing.IpAddressCidr = DeviceFingerprint.NormalizeIp(ipAddress);
            return await _repo.UpsertAsync(existing, ct);
        }

        var device = new TrustedDevice
        {
            UserId = userId,
            DeviceId = NewPublicDeviceId(),
            FingerprintHash = fingerprint,
            DeviceName = DeviceFingerprint.DeriveName(userAgent),
            UserAgent = userAgent.Length > 500 ? userAgent[..500] : userAgent,
            IpAddressCidr = DeviceFingerprint.NormalizeIp(ipAddress),
            FirstSeenAtUtc = _clock.GetUtcNow().UtcDateTime,
            LastSeenAtUtc = _clock.GetUtcNow().UtcDateTime,
        };
        return await _repo.UpsertAsync(device, ct);
    }

    public Task<TrustedDevice?> FindByFingerprintAsync(Guid userId, string fingerprintHash, CancellationToken ct = default)
        => _repo.FindByFingerprintAsync(userId, fingerprintHash, ct);

    public Task<TrustedDevice?> FindByPublicIdAsync(Guid userId, string deviceId, CancellationToken ct = default)
        => _repo.FindByPublicIdAsync(userId, deviceId, ct);

    public async Task MarkUsedAsync(int trustedDeviceId, bool usedWebAuthn, CancellationToken ct = default)
    {
        // Load the row by primary key, stamp the appropriate field, and
        // persist. We don't expose a public mark-by-public-id because the
        // only callers (WebAuthnController) already know the row id from
        // a prior lookup.
        var row = await _repo.FindByIdAsync(trustedDeviceId, ct);
        if (row is null || row.IsRevoked) return;

        var now = _clock.GetUtcNow().UtcDateTime;
        row.LastSeenAtUtc = now;
        if (usedWebAuthn)
        {
            row.LastWebAuthnAtUtc = now;
        }
        await _repo.UpsertAsync(row, ct);
    }

    public Task<IReadOnlyList<TrustedDevice>> ListAsync(Guid userId, CancellationToken ct = default)
        => _repo.ListAsync(userId, ct);

    public Task<bool> RevokeAsync(Guid userId, string deviceId, CancellationToken ct = default)
        => _repo.RevokeAsync(userId, deviceId, ct);

    public Task<bool> TrustAsync(Guid userId, string deviceId, TimeSpan window, CancellationToken ct = default)
        => _repo.SetTrustedAsync(userId, deviceId, _clock.GetUtcNow().UtcDateTime + window, ct);

    /// <summary>32-char URL-safe base64 of 24 random bytes — collision-safe opaque device id.</summary>
    internal static string NewPublicDeviceId()
    {
        Span<byte> raw = stackalloc byte[24];
        RandomNumberGenerator.Fill(raw);
        return Convert.ToBase64String(raw)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}

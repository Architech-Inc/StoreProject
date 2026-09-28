using Microsoft.AspNetCore.Http;
using Moq;
using Store.DbServices.Services;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;
using Store.Models.Security;

namespace Store.API.Tests;

/// <summary>
/// SEC-23 — tests for <see cref="TrustedDeviceService"/>. The service
/// orchestrates fingerprint derivation from <see cref="HttpContext"/>
/// + persistence via the (mocked) <see cref="ITrustedDeviceRepository"/>.
/// All randomness / clock behavior is controlled so tests stay deterministic.
/// </summary>
public class TrustedDeviceServiceTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-2222-3333-4444-555555555555");

    private static HttpContext BuildContext(string userAgent, string? ip, string acceptLanguage)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers.UserAgent = userAgent;
        if (ip is not null)
        {
            ctx.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(ip);
        }
        ctx.Request.Headers.AcceptLanguage = acceptLanguage;
        return ctx;
    }

    private static TrustedDeviceService BuildService(
        Mock<ITrustedDeviceRepository> repo,
        DateTime? nowUtc = null)
    {
        var clock = new FixedTimeProvider(nowUtc ?? new DateTime(2026, 9, 25, 12, 0, 0, DateTimeKind.Utc));
        return new TrustedDeviceService(repo.Object, clock);
    }

    private static Mock<ITrustedDeviceRepository> NewRepo() => new(MockBehavior.Strict);

    // ---- New-device registration ----

    [Fact]
    public async Task RegisterOrUpdateAsync_creates_new_device_when_fingerprint_unknown()
    {
        var repo = NewRepo();
        repo.Setup(r => r.FindByFingerprintAsync(UserId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TrustedDevice?)null);
        TrustedDevice? captured = null;
        repo.Setup(r => r.UpsertAsync(It.IsAny<TrustedDevice>(), It.IsAny<CancellationToken>()))
            .Callback<TrustedDevice, CancellationToken>((d, _) => captured = d)
            .ReturnsAsync((TrustedDevice d, CancellationToken _) => d);

        var service = BuildService(repo);
        var ctx = BuildContext("Mozilla/5.0 (Macintosh)", "10.0.0.5", "en-US");

        var result = await service.RegisterOrUpdateAsync(UserId, ctx);

        Assert.NotNull(captured);
        Assert.Equal(UserId, captured!.UserId);
        Assert.NotEmpty(captured.DeviceId);
        Assert.Equal(32, captured.DeviceId.Length); // 24 bytes base64url unpadded
        Assert.Equal("macOS", captured.DeviceName);
        Assert.Equal("10.0.0.0/24", captured.IpAddressCidr);
        Assert.False(captured.IsRevoked);
        Assert.False(captured.IsTrusted);
        Assert.Equal(captured.DeviceId, result.DeviceId);
        repo.Verify(r => r.UpsertAsync(It.IsAny<TrustedDevice>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterOrUpdateAsync_uses_lower_64_hex_chars_for_fingerprint()
    {
        var repo = NewRepo();
        string? capturedFingerprint = null;
        repo.Setup(r => r.FindByFingerprintAsync(UserId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, string, CancellationToken>((_, fp, _) => capturedFingerprint = fp)
            .ReturnsAsync((TrustedDevice?)null);
        repo.Setup(r => r.UpsertAsync(It.IsAny<TrustedDevice>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TrustedDevice d, CancellationToken _) => d);

        var service = BuildService(repo);
        var ctx = BuildContext("ua", "10.0.0.1", "en-US");

        await service.RegisterOrUpdateAsync(UserId, ctx);

        Assert.NotNull(capturedFingerprint);
        Assert.Equal(64, capturedFingerprint!.Length);
        Assert.Matches("^[0-9a-f]{64}$", capturedFingerprint);
    }

    // ---- Known-device update ----

    [Fact]
    public async Task RegisterOrUpdateAsync_updates_existing_device_in_place()
    {
        var now = new DateTime(2026, 9, 25, 12, 0, 0, DateTimeKind.Utc);
        var existing = new TrustedDevice
        {
            TrustedDeviceId = 7,
            UserId = UserId,
            DeviceId = "existing-device-id",
            FingerprintHash = DeviceFingerprint.ComputeHash("ua", "10.0.0.1", "en-US"),
            DeviceName = "stale name",
            FirstSeenAtUtc = now.AddDays(-30),
            LastSeenAtUtc = now.AddDays(-30),
        };

        var repo = NewRepo();
        repo.Setup(r => r.FindByFingerprintAsync(UserId, existing.FingerprintHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        TrustedDevice? updated = null;
        repo.Setup(r => r.UpsertAsync(It.IsAny<TrustedDevice>(), It.IsAny<CancellationToken>()))
            .Callback<TrustedDevice, CancellationToken>((d, _) => updated = d)
            .ReturnsAsync((TrustedDevice d, CancellationToken _) => d);

        var service = BuildService(repo, now);
        var ctx = BuildContext("ua", "10.0.0.1", "en-US");

        var result = await service.RegisterOrUpdateAsync(UserId, ctx);

        Assert.Same(existing, result);
        Assert.NotNull(updated);
        Assert.Equal(7, updated!.TrustedDeviceId);
        Assert.Equal("existing-device-id", updated.DeviceId); // public id stable across updates
        Assert.Equal(now, updated.LastSeenAtUtc);
        repo.Verify(r => r.UpsertAsync(It.IsAny<TrustedDevice>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterOrUpdateAsync_truncates_long_user_agent_to_500_chars()
    {
        var longUa = new string('a', 1000);
        var repo = NewRepo();
        repo.Setup(r => r.FindByFingerprintAsync(UserId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TrustedDevice?)null);
        TrustedDevice? captured = null;
        repo.Setup(r => r.UpsertAsync(It.IsAny<TrustedDevice>(), It.IsAny<CancellationToken>()))
            .Callback<TrustedDevice, CancellationToken>((d, _) => captured = d)
            .ReturnsAsync((TrustedDevice d, CancellationToken _) => d);

        var service = BuildService(repo);
        var ctx = BuildContext(longUa, "10.0.0.1", "en-US");

        await service.RegisterOrUpdateAsync(UserId, ctx);

        Assert.NotNull(captured);
        Assert.NotNull(captured!.UserAgent);
        Assert.Equal(500, captured.UserAgent!.Length);
    }

    // ---- Pass-throughs ----

    [Fact]
    public async Task ListAsync_returns_repository_results()
    {
        var repo = NewRepo();
        var rows = new List<TrustedDevice>
        {
            new() { TrustedDeviceId = 1, DeviceId = "a", DeviceName = "A" },
            new() { TrustedDeviceId = 2, DeviceId = "b", DeviceName = "B" },
        };
        repo.Setup(r => r.ListAsync(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(rows);

        var service = BuildService(repo);
        var result = await service.ListAsync(UserId);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task RevokeAsync_forwards_to_repository()
    {
        var repo = NewRepo();
        repo.Setup(r => r.RevokeAsync(UserId, "abc", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var service = BuildService(repo);
        var ok = await service.RevokeAsync(UserId, "abc");

        Assert.True(ok);
        repo.Verify(r => r.RevokeAsync(UserId, "abc", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TrustAsync_forwards_to_repository_with_correct_window()
    {
        var now = new DateTime(2026, 9, 25, 12, 0, 0, DateTimeKind.Utc);
        var repo = NewRepo();
        DateTime? capturedUntil = null;
        repo.Setup(r => r.SetTrustedAsync(UserId, "abc", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, string, DateTime, CancellationToken>((_, _, u, _) => capturedUntil = u)
            .ReturnsAsync(true);

        var service = BuildService(repo, now);
        var ok = await service.TrustAsync(UserId, "abc", TimeSpan.FromDays(7));

        Assert.True(ok);
        Assert.NotNull(capturedUntil);
        Assert.Equal(now.AddDays(7), capturedUntil!.Value);
    }

    [Fact]
    public async Task FindByFingerprintAsync_forwards_to_repository()
    {
        var fp = DeviceFingerprint.ComputeHash("ua", "10.0.0.1", "en-US");
        var repo = NewRepo();
        var row = new TrustedDevice { TrustedDeviceId = 5, FingerprintHash = fp };
        repo.Setup(r => r.FindByFingerprintAsync(UserId, fp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(row);

        var service = BuildService(repo);
        var result = await service.FindByFingerprintAsync(UserId, fp);

        Assert.Same(row, result);
    }

    // ---- Helpers ----

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;
        public FixedTimeProvider(DateTime utcNow) { _now = new DateTimeOffset(utcNow, TimeSpan.Zero); }
        public override DateTimeOffset GetUtcNow() => _now;
    }

    // ---- MarkUsedAsync (SEC-23 WebAuthn stamp) ----

    [Fact]
    public async Task MarkUsedAsync_with_webauthn_stamps_LastWebAuthnAtUtc()
    {
        var now = new DateTime(2026, 9, 25, 12, 0, 0, DateTimeKind.Utc);
        var row = new TrustedDevice
        {
            TrustedDeviceId = 9,
            UserId = UserId,
            DeviceId = "device-9",
            FingerprintHash = "fp",
            LastSeenAtUtc = now.AddDays(-1),
            LastWebAuthnAtUtc = null,
        };
        var repo = NewRepo();
        repo.Setup(r => r.FindByIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(row);
        TrustedDevice? stamped = null;
        repo.Setup(r => r.UpsertAsync(It.IsAny<TrustedDevice>(), It.IsAny<CancellationToken>()))
            .Callback<TrustedDevice, CancellationToken>((d, _) => stamped = d)
            .ReturnsAsync((TrustedDevice d, CancellationToken _) => d);

        var service = BuildService(repo, now);
        await service.MarkUsedAsync(9, usedWebAuthn: true);

        Assert.NotNull(stamped);
        Assert.Equal(now, stamped!.LastSeenAtUtc);
        Assert.Equal(now, stamped.LastWebAuthnAtUtc);
    }

    [Fact]
    public async Task MarkUsedAsync_without_webauthn_only_bumps_LastSeen()
    {
        var now = new DateTime(2026, 9, 25, 12, 0, 0, DateTimeKind.Utc);
        var priorWebAuthn = now.AddDays(-7);
        var row = new TrustedDevice
        {
            TrustedDeviceId = 10,
            UserId = UserId,
            DeviceId = "device-10",
            FingerprintHash = "fp",
            LastSeenAtUtc = now.AddDays(-1),
            LastWebAuthnAtUtc = priorWebAuthn,
        };
        var repo = NewRepo();
        repo.Setup(r => r.FindByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(row);
        TrustedDevice? stamped = null;
        repo.Setup(r => r.UpsertAsync(It.IsAny<TrustedDevice>(), It.IsAny<CancellationToken>()))
            .Callback<TrustedDevice, CancellationToken>((d, _) => stamped = d)
            .ReturnsAsync((TrustedDevice d, CancellationToken _) => d);

        var service = BuildService(repo, now);
        await service.MarkUsedAsync(10, usedWebAuthn: false);

        Assert.NotNull(stamped);
        Assert.Equal(now, stamped!.LastSeenAtUtc);
        Assert.Equal(priorWebAuthn, stamped.LastWebAuthnAtUtc); // unchanged
    }

    [Fact]
    public async Task MarkUsedAsync_skips_revoked_device()
    {
        var row = new TrustedDevice
        {
            TrustedDeviceId = 11,
            UserId = UserId,
            DeviceId = "device-11",
            FingerprintHash = "fp",
            IsRevoked = true,
        };
        var repo = NewRepo();
        repo.Setup(r => r.FindByIdAsync(11, It.IsAny<CancellationToken>())).ReturnsAsync(row);

        var service = BuildService(repo);
        await service.MarkUsedAsync(11, usedWebAuthn: true);

        repo.Verify(r => r.UpsertAsync(It.IsAny<TrustedDevice>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkUsedAsync_skips_missing_row()
    {
        var repo = NewRepo();
        repo.Setup(r => r.FindByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((TrustedDevice?)null);

        var service = BuildService(repo);
        await service.MarkUsedAsync(99, usedWebAuthn: true);

        repo.Verify(r => r.UpsertAsync(It.IsAny<TrustedDevice>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

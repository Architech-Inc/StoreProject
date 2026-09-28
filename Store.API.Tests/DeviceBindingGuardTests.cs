using Microsoft.AspNetCore.Http;
using Moq;
using Store.DbServices.Services;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;
using Store.Models.Security;

namespace Store.API.Tests;

/// <summary>
/// SEC-23 — tests for <see cref="DeviceBindingGuard"/>. Verifies the
/// verdict mapping (EnrolledWithWebAuthn / PasswordOnly / UnknownDevice /
/// RevokedDevice) and the fail-closed behavior on degenerate requests.
/// </summary>
public class DeviceBindingGuardTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-2222-3333-4444-555555555555");

    private static HttpContext BuildContext(string userAgent, string ip, string lang)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers.UserAgent = userAgent;
        ctx.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(ip);
        ctx.Request.Headers.AcceptLanguage = lang;
        return ctx;
    }

    private static DeviceBindingGuard BuildGuard(Mock<ITrustedDeviceService> svc)
        => new(svc.Object);

    // ---- Known device + LastWebAuthnAtUtc present ----

    [Fact]
    public async Task CheckAsync_returns_EnrolledWithWebAuthn_when_known_and_webauthn_seen()
    {
        var fingerprint = DeviceFingerprint.ComputeHash("ua", "10.0.0.1", "en-US");
        var svc = new Mock<ITrustedDeviceService>();
        svc.Setup(s => s.FindByFingerprintAsync(UserId, fingerprint, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TrustedDevice
            {
                TrustedDeviceId = 1,
                FingerprintHash = fingerprint,
                IsRevoked = false,
                LastWebAuthnAtUtc = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            });

        var guard = BuildGuard(svc);
        var result = await guard.CheckAsync(UserId, BuildContext("ua", "10.0.0.1", "en-US"));

        Assert.Equal(DeviceBindingResult.EnrolledWithWebAuthn, result);
    }

    // ---- Known device but never WebAuthn ----

    [Fact]
    public async Task CheckAsync_returns_PasswordOnly_when_known_but_never_used_webauthn()
    {
        var fingerprint = DeviceFingerprint.ComputeHash("ua", "10.0.0.1", "en-US");
        var svc = new Mock<ITrustedDeviceService>();
        svc.Setup(s => s.FindByFingerprintAsync(UserId, fingerprint, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TrustedDevice
            {
                TrustedDeviceId = 2,
                FingerprintHash = fingerprint,
                IsRevoked = false,
                LastWebAuthnAtUtc = null,
            });

        var guard = BuildGuard(svc);
        var result = await guard.CheckAsync(UserId, BuildContext("ua", "10.0.0.1", "en-US"));

        Assert.Equal(DeviceBindingResult.PasswordOnly, result);
    }

    // ---- Unknown fingerprint ----

    [Fact]
    public async Task CheckAsync_returns_UnknownDevice_when_fingerprint_not_seen_before()
    {
        var svc = new Mock<ITrustedDeviceService>();
        svc.Setup(s => s.FindByFingerprintAsync(UserId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TrustedDevice?)null);

        var guard = BuildGuard(svc);
        var result = await guard.CheckAsync(UserId, BuildContext("ua", "10.0.0.1", "en-US"));

        Assert.Equal(DeviceBindingResult.UnknownDevice, result);
    }

    // ---- Soft-revoked device ----

    [Fact]
    public async Task CheckAsync_returns_RevokedDevice_when_device_is_revoked()
    {
        var fingerprint = DeviceFingerprint.ComputeHash("ua", "10.0.0.1", "en-US");
        var svc = new Mock<ITrustedDeviceService>();
        svc.Setup(s => s.FindByFingerprintAsync(UserId, fingerprint, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TrustedDevice
            {
                TrustedDeviceId = 3,
                FingerprintHash = fingerprint,
                IsRevoked = true,
                LastWebAuthnAtUtc = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            });

        var guard = BuildGuard(svc);
        var result = await guard.CheckAsync(UserId, BuildContext("ua", "10.0.0.1", "en-US"));

        Assert.Equal(DeviceBindingResult.RevokedDevice, result);
    }

    // ---- IP rotation inside /24 still matches ----

    [Fact]
    public async Task CheckAsync_treats_same_24_subnet_as_same_device()
    {
        // Fingerprint computed at register-time used 10.0.0.1; refresh
        // comes from 10.0.0.99 — same /24, same fingerprint, same row.
        var registerFingerprint = DeviceFingerprint.ComputeHash("ua", "10.0.0.1", "en-US");
        var svc = new Mock<ITrustedDeviceService>();
        svc.Setup(s => s.FindByFingerprintAsync(UserId, registerFingerprint, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TrustedDevice
            {
                TrustedDeviceId = 4,
                FingerprintHash = registerFingerprint,
                IsRevoked = false,
                LastWebAuthnAtUtc = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            });

        var guard = BuildGuard(svc);
        var result = await guard.CheckAsync(UserId, BuildContext("ua", "10.0.0.99", "en-US"));

        Assert.Equal(DeviceBindingResult.EnrolledWithWebAuthn, result);
    }

    // ---- Degenerate request fails closed ----

    [Fact]
    public async Task CheckAsync_returns_UnknownDevice_when_no_headers_present()
    {
        var svc = new Mock<ITrustedDeviceService>(MockBehavior.Strict);
        var ctx = new DefaultHttpContext(); // empty — no UA, no IP, no Accept-Language

        var guard = BuildGuard(svc);
        var result = await guard.CheckAsync(UserId, ctx);

        Assert.Equal(DeviceBindingResult.UnknownDevice, result);
        svc.VerifyNoOtherCalls();
    }
}

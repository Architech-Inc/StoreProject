using Microsoft.AspNetCore.Http;
using Store.Models.Interfaces.Services;
using Store.Models.Security;

namespace Store.DbServices.Services;

/// <summary>
/// SEC-23 — default <see cref="IDeviceBindingGuard"/>. Reads the request
/// fingerprint from <see cref="HttpContext"/>, hashes it via
/// <see cref="DeviceFingerprint"/>, and asks
/// <see cref="ITrustedDeviceService.FindByFingerprintAsync"/> whether the
/// device is known to the user.
///
/// Stateless; safe to register as a singleton.
/// </summary>
public class DeviceBindingGuard : IDeviceBindingGuard
{
    private readonly ITrustedDeviceService _devices;

    public DeviceBindingGuard(ITrustedDeviceService devices)
    {
        _devices = devices;
    }

    public async Task<DeviceBindingResult> CheckAsync(Guid userId, HttpContext httpContext, CancellationToken ct = default)
    {
        var fingerprint = ComputeFingerprint(httpContext);
        if (fingerprint is null)
        {
            // No IP / UA — treat as unknown so we fail closed rather than
            // than silently letting a degenerate request through.
            return DeviceBindingResult.UnknownDevice;
        }

        var device = await _devices.FindByFingerprintAsync(userId, fingerprint, ct);
        if (device is null)
        {
            return DeviceBindingResult.UnknownDevice;
        }

        if (device.IsRevoked)
        {
            return DeviceBindingResult.RevokedDevice;
        }

        return device.LastWebAuthnAtUtc.HasValue
            ? DeviceBindingResult.EnrolledWithWebAuthn
            : DeviceBindingResult.PasswordOnly;
    }

    /// <summary>
    /// Re-derive the fingerprint from the live request. Public so the
    /// WebAuthnController can use it to mark the "this device" badge
    /// when listing the user's enrolled devices.
    /// </summary>
    public static string? ComputeFingerprint(HttpContext httpContext)
    {
        var ua = httpContext.Request.Headers.UserAgent.ToString();
        var ip = httpContext.Connection.RemoteIpAddress?.ToString();
        var lang = httpContext.Request.Headers.AcceptLanguage.ToString();
        if (string.IsNullOrWhiteSpace(ua) && string.IsNullOrWhiteSpace(ip) && string.IsNullOrWhiteSpace(lang))
        {
            return null;
        }
        return DeviceFingerprint.ComputeHash(ua, ip, lang);
    }
}

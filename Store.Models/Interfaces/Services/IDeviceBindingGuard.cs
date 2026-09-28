using Microsoft.AspNetCore.Http;
using Store.Models.Security;

namespace Store.Models.Interfaces.Services;

/// <summary>
/// SEC-23 — security check that decides whether a request comes from an
/// enrolled device. Called from <c>AuthenticationService.RefreshTokenAsync</c>
/// and the WebAuthn assertion path. Implementations are stateless and
/// resolve the fingerprint from the live <see cref="HttpContext"/>.
///
/// Returns a <see cref="DeviceBindingResult"/> verdict rather than a
/// boolean so callers can distinguish "unknown device" from "known but
/// password-only" (different UX responses).
/// </summary>
public interface IDeviceBindingGuard
{
    Task<DeviceBindingResult> CheckAsync(Guid userId, HttpContext httpContext, CancellationToken ct = default);
}

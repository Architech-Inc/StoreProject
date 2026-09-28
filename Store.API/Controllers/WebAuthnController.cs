using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Store.DbServices.Services;
using Store.Models.Interfaces.Services;
using Fido2NetLib;
using Store.Models.DTOs.Auth;
using Store.Models.DTOs.Common;
using Store.Models.Security;
using System.Security.Claims;

using Microsoft.Extensions.Logging;
using Store.Models.Common;
namespace Store.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebAuthnController : ControllerBase
{
    private readonly ILogger<WebAuthnController> _logger;
    private readonly IWebAuthnService _webAuthnService;
    // SEC-23 — device enrollment. Both nullable so the controller stays
    // constructable when the device layer has not been wired (e.g. some
    // test hosts / ControlPlane); the assertion path will then skip the
    // device-marking step.
    private readonly ITrustedDeviceService? _trustedDevices;

    public WebAuthnController(
        IWebAuthnService webAuthnService,
        ILogger<WebAuthnController> logger,
        ITrustedDeviceService? trustedDevices = null)
    {
        _webAuthnService = webAuthnService;
        _logger = logger;
        _trustedDevices = trustedDevices;
    }

    [Authorize]
    [HttpPost("makeCredentialOptions")]
    [EnableRateLimiting("webauthn-assertion")]
    public async Task<IActionResult> MakeCredentialOptions(CancellationToken ct)
    {
        var userIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized(new { message = "Invalid user token" });

        var options = await _webAuthnService.RequestNewCredentialAsync(userId, ct);
        return Ok(options);
    }

    [Authorize]
    [HttpPost("makeCredential")]
    [EnableRateLimiting("webauthn-assertion")]
    public async Task<IActionResult> MakeCredential([FromBody] AuthenticatorAttestationRawResponse response, CancellationToken ct)
    {
        try
        {
            var userIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
                return Unauthorized(new { message = "Invalid user token" });

            var result = await _webAuthnService.RegisterNewCredentialAsync(userId, response, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = SafeErrorMessage.From(ex, _logger, "Web Authn operation") });
        }
    }

    public class AssertionOptionsRequest
    {
        public string Username { get; set; } = string.Empty;
    }

    [AllowAnonymous]
    [HttpPost("assertionOptions")]
    [EnableRateLimiting("webauthn-assertion")]
    public async Task<IActionResult> AssertionOptions([FromBody] AssertionOptionsRequest request, CancellationToken ct)
    {
        try
        {
            var options = await _webAuthnService.RequestAssertionAsync(request.Username, ct);
            return Ok(options);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = SafeErrorMessage.From(ex, _logger, "Web Authn operation") });
        }
    }

    [AllowAnonymous]
    [HttpPost("makeAssertion")]
    [EnableRateLimiting("webauthn-assertion")]
    public async Task<IActionResult> MakeAssertion(
        [FromBody] AuthenticatorAssertionRawResponse response,
        [FromServices] Store.API.Application.Auth.Ports.IAuthPort authPort,
        CancellationToken ct)
    {
        try
        {
            var (result, userId) = await _webAuthnService.MakeAssertionAsync(response, ct);

            // SEC-23 — enroll / stamp the calling device BEFORE issuing a
            // session. The FIDO2 credential itself is already bound to the
            // user's authenticator; what we add is *device* binding — so a
            // stolen refresh token cannot be replayed from a brand-new
            // device (which would still have the user's authenticator if
            // they had physical access — the "compromised shared device"
            // scenario). If RegisterOrUpdateAsync returns a row we know
            // existed before the assertion (TrustedDeviceId != 0) we
            // stamp LastWebAuthnAtUtc to mark the device as fully
            // WebAuthn-enrolled; brand-new devices get a fresh row with
            // LastWebAuthnAtUtc stamped here.
            if (_trustedDevices is not null)
            {
                try
                {
                    var device = await _trustedDevices.RegisterOrUpdateAsync(userId, HttpContext, ct);
                    if (device is not null)
                    {
                        await _trustedDevices.MarkUsedAsync(device.TrustedDeviceId, usedWebAuthn: true, ct);
                    }
                }
                catch (Exception devEx)
                {
                    // Device-marking failure must not block the user
                    // from logging in. Log and continue.
                    _logger.LogWarning(devEx,
                        "SEC-23 — device enrollment failed for user {UserId}; login continues",
                        userId);
                }
            }

            var loginResponse = await authPort.LoginWithBiometricsAsync(userId, ct);
            if (loginResponse == null) return Unauthorized(new { message = "Biometric authentication succeeded, but account is invalid" });

            // Mimic the normal Auth login response JSON format for compatibility
            return Ok(new { success = true, data = loginResponse });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = SafeErrorMessage.From(ex, _logger, "Web Authn operation") });
        }
    }

    [Authorize]
    [HttpGet("credentials")]
    public async Task<IActionResult> GetCredentials(CancellationToken ct)
    {
        try
        {
            var userIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
                return Unauthorized(new { message = "Invalid user token" });

            var credentials = await _webAuthnService.GetCredentialsAsync(userId, ct);
            return Ok(Store.Models.DTOs.Common.ApiResponse<List<Store.Models.DTOs.Auth.FidoCredentialDto>>.Ok(credentials));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = SafeErrorMessage.From(ex, _logger, "Web Authn operation") });
        }
    }

    [Authorize]
    [HttpDelete("credentials/{id}")]
    public async Task<IActionResult> DeleteCredential(int id, CancellationToken ct)
    {
        try
        {
            var userIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
                return Unauthorized(new { message = "Invalid user token" });

            var success = await _webAuthnService.RemoveCredentialAsync(userId, id, ct);
            if (!success)
                return NotFound(new { message = "Credential not found" });

            return Ok(Store.Models.DTOs.Common.ApiResponse<object>.Ok(null!, "Credential removed."));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = SafeErrorMessage.From(ex, _logger, "Web Authn operation") });
        }
    }

    // ============================================================
    //  SEC-23 — Enrolled-device management
    // ============================================================

    /// <summary>
    /// List every non-revoked device the calling user has enrolled. Marks
    /// the row that matches the current request's fingerprint so the UI
    /// can render "this device" badges without revealing the hash.
    /// </summary>
    [Authorize]
    [HttpGet("devices")]
    public async Task<IActionResult> ListDevices(CancellationToken ct)
    {
        var userIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized(new { message = "Invalid user token" });

        if (_trustedDevices is null)
        {
            // Device layer not wired in this host — return an empty list
            // rather than 500 so the Profile page renders gracefully.
            return Ok(ApiResponse<IReadOnlyList<TrustedDeviceDto>>.Ok(Array.Empty<TrustedDeviceDto>()));
        }

        var devices = await _trustedDevices.ListAsync(userId, ct);
        var currentFingerprint = DeviceBindingGuard.ComputeFingerprint(HttpContext);
        var dtos = devices.Select(d => new TrustedDeviceDto
        {
            DeviceId = d.DeviceId,
            DeviceName = d.DeviceName,
            UserAgent = d.UserAgent,
            FirstSeenAtUtc = d.FirstSeenAtUtc,
            LastSeenAtUtc = d.LastSeenAtUtc,
            LastWebAuthnAtUtc = d.LastWebAuthnAtUtc,
            IsTrusted = d.IsTrusted,
            TrustedUntilUtc = d.TrustedUntilUtc,
            IsCurrentDevice = currentFingerprint is not null
                && string.Equals(currentFingerprint, d.FingerprintHash, StringComparison.Ordinal),
        }).ToList();

        return Ok(ApiResponse<IReadOnlyList<TrustedDeviceDto>>.Ok(dtos));
    }

    /// <summary>
    /// Revoke the device identified by its public id. Subsequent refresh
    /// tokens from that fingerprint will be denied; the user must
    /// re-authenticate (and re-enroll) from the device.
    /// </summary>
    [Authorize]
    [HttpDelete("devices/{deviceId}")]
    public async Task<IActionResult> RevokeDevice(string deviceId, CancellationToken ct)
    {
        var userIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized(new { message = "Invalid user token" });

        if (_trustedDevices is null)
            return StatusCode(StatusCodes.Status501NotImplemented,
                new { message = "Device management not configured for this host." });

        var ok = await _trustedDevices.RevokeAsync(userId, deviceId, ct);
        if (!ok)
            return NotFound(new { message = "Device not found." });

        _logger.LogInformation("SEC-23 — user {UserId} revoked device {DeviceId}", userId, deviceId);
        return Ok(ApiResponse<object>.Ok(null!, "Device revoked."));
    }

    /// <summary>
    /// Promote a device to operator-trusted for 30 days. A trusted device
    /// is allowed to refresh from new IPs without re-WebAuthn. Useful for
    /// shared office workstations where the IP rotates.
    /// </summary>
    [Authorize]
    [HttpPost("devices/{deviceId}/trust")]
    public async Task<IActionResult> TrustDevice(string deviceId, CancellationToken ct)
    {
        var userIdString = User.FindFirstValue("uid") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized(new { message = "Invalid user token" });

        if (_trustedDevices is null)
            return StatusCode(StatusCodes.Status501NotImplemented,
                new { message = "Device management not configured for this host." });

        var ok = await _trustedDevices.TrustAsync(userId, deviceId, TimeSpan.FromDays(30), ct);
        if (!ok)
            return NotFound(new { message = "Device not found." });

        _logger.LogInformation("SEC-23 — user {UserId} trusted device {DeviceId} for 30 days", userId, deviceId);
        return Ok(ApiResponse<object>.Ok(null!, "Device trusted for 30 days."));
    }
}

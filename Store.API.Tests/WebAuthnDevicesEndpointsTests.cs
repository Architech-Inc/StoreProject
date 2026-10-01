using System.Security.Claims;
using Fido2NetLib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Store.API.Controllers;
using Store.Models.DTOs.Auth;
using Store.Models.DTOs.Common;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;

namespace Store.API.Tests;

/// <summary>
/// SEC-23 — tests for the /api/webauthn/devices endpoints
/// (ListDevices / RevokeDevice / TrustDevice). The WebAuthnController's
/// device layer is optional — when not registered, ListDevices returns an
/// empty array and Revoke / Trust return 501 — so the controller stays
/// usable in hosts without the device layer.
/// </summary>
public class WebAuthnDevicesEndpointsTests
{
    private static readonly Guid UserId = Guid.Parse("aaaaaaaa-bddd-cccc-eeee-ffffffffffff");

    private static WebAuthnController BuildController(
        Mock<ITrustedDeviceService>? devices = null,
        HttpContext? httpContext = null)
    {
        var webAuthn = new Mock<IWebAuthnService>(MockBehavior.Strict);
        var controller = new WebAuthnController(
            webAuthn.Object,
            NullLogger<WebAuthnController>.Instance,
            devices?.Object);

        var ctx = httpContext ?? new DefaultHttpContext();
        var claims = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("uid", UserId.ToString()),
        }, "TestAuth"));
        ctx.User = claims;
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };
        return controller;
    }

    // ---- ListDevices ----

    [Fact]
    public async Task ListDevices_returns_empty_array_when_device_layer_not_wired()
    {
        var controller = BuildController(devices: null);

        var result = await controller.ListDevices(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = ok.Value as ApiResponse<IReadOnlyList<TrustedDeviceDto>>;
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.NotNull(payload.Data);
        Assert.Empty(payload.Data);
    }

    [Fact]
    public async Task ListDevices_marks_current_device_when_fingerprint_matches()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers.UserAgent = "ua-current";
        ctx.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.0.0.5");
        ctx.Request.Headers.AcceptLanguage = "en-US";
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("uid", UserId.ToString()) }, "TestAuth"));

        var currentFingerprint = Store.Models.Security.DeviceFingerprint.ComputeHash("ua-current", "10.0.0.5", "en-US");

        var svc = new Mock<ITrustedDeviceService>();
        svc.Setup(s => s.ListAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TrustedDevice>
            {
                new() { TrustedDeviceId = 1, DeviceId = "d1", DeviceName = "A", FingerprintHash = currentFingerprint },
                new() { TrustedDeviceId = 2, DeviceId = "d2", DeviceName = "B", FingerprintHash = "different-fp" },
            });

        var controller = new WebAuthnController(
            new Mock<IWebAuthnService>(MockBehavior.Strict).Object,
            NullLogger<WebAuthnController>.Instance,
            svc.Object);
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        var result = await controller.ListDevices(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = ok.Value as ApiResponse<IReadOnlyList<TrustedDeviceDto>>;
        Assert.NotNull(payload);
        var list = payload!.Data!.ToList();
        Assert.Equal(2, list.Count);
        Assert.Single(list, d => d.IsCurrentDevice && d.DeviceName == "A");
        Assert.Single(list, d => !d.IsCurrentDevice && d.DeviceName == "B");
    }

    [Fact]
    public async Task ListDevices_returns_401_when_uid_claim_missing()
    {
        var ctx = new DefaultHttpContext(); // no user
        var controller = new WebAuthnController(
            new Mock<IWebAuthnService>(MockBehavior.Strict).Object,
            NullLogger<WebAuthnController>.Instance,
            new Mock<ITrustedDeviceService>().Object);
        controller.ControllerContext = new ControllerContext { HttpContext = ctx };

        var result = await controller.ListDevices(CancellationToken.None);
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    // ---- RevokeDevice ----

    [Fact]
    public async Task RevokeDevice_returns_501_when_device_layer_not_wired()
    {
        var controller = BuildController(devices: null);

        var result = await controller.RevokeDevice("d1", CancellationToken.None);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status501NotImplemented, status.StatusCode);
    }

    [Fact]
    public async Task RevokeDevice_returns_404_when_service_returns_false()
    {
        var svc = new Mock<ITrustedDeviceService>();
        svc.Setup(s => s.RevokeAsync(UserId, "missing", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var controller = BuildController(devices: svc);

        var result = await controller.RevokeDevice("missing", CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task RevokeDevice_returns_200_when_service_returns_true()
    {
        var svc = new Mock<ITrustedDeviceService>();
        svc.Setup(s => s.RevokeAsync(UserId, "d1", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var controller = BuildController(devices: svc);

        var result = await controller.RevokeDevice("d1", CancellationToken.None);

        Assert.IsType<OkObjectResult>(result);
    }

    // ---- TrustDevice ----

    [Fact]
    public async Task TrustDevice_returns_404_when_service_returns_false()
    {
        var svc = new Mock<ITrustedDeviceService>();
        svc.Setup(s => s.TrustAsync(UserId, "missing", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var controller = BuildController(devices: svc);

        var result = await controller.TrustDevice("missing", CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task TrustDevice_passes_30_day_window_to_service()
    {
        TimeSpan? capturedWindow = null;
        var svc = new Mock<ITrustedDeviceService>();
        svc.Setup(s => s.TrustAsync(UserId, "d1", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, string, TimeSpan, CancellationToken>((_, _, w, _) => capturedWindow = w)
            .ReturnsAsync(true);

        var controller = BuildController(devices: svc);

        await controller.TrustDevice("d1", CancellationToken.None);

        Assert.NotNull(capturedWindow);
        Assert.Equal(TimeSpan.FromDays(30), capturedWindow!.Value);
    }
}

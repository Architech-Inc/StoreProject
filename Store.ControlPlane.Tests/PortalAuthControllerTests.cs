using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Store.ControlPlane.Controllers;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Services;
using Store.Models.DTOs.Common;
using Xunit;

namespace Store.ControlPlane.Tests;

public class PortalAuthControllerTests
{
    private readonly Mock<IPortalAuthService> _authServiceMock;
    private readonly Mock<ILogger<PortalAuthController>> _loggerMock;
    private readonly PortalAuthController _controller;

    public PortalAuthControllerTests()
    {
        _authServiceMock = new Mock<IPortalAuthService>();
        _loggerMock = new Mock<ILogger<PortalAuthController>>();
        _controller = new PortalAuthController(_authServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsCreated()
    {
        var request = new RegisterPortalAccountRequest("owner@example.com", "Jane Owner", "SecurePassword123!");
        var authResponse = new PortalAuthResponse(
            Guid.NewGuid(),
            "owner@example.com",
            "Jane Owner",
            Guid.NewGuid(),
            "jane-store",
            "Jane Store",
            "session-token-xyz",
            DateTime.UtcNow.AddDays(7));

        _authServiceMock
            .Setup(a => a.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        var result = await _controller.Register(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(201, objectResult.StatusCode);
        var response = Assert.IsType<ApiResponse<PortalAuthResponse>>(objectResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("owner@example.com", response.Data.Email);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        var request = new RegisterPortalAccountRequest("existing@example.com", "Jane Owner", "SecurePassword123!");

        _authServiceMock
            .Setup(a => a.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("An account with this email already exists."));

        var result = await _controller.Register(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequest.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        var request = new LoginPortalAccountRequest("owner@example.com", "SecurePassword123!");
        var authResponse = new PortalAuthResponse(
            Guid.NewGuid(),
            "owner@example.com",
            "Jane Owner",
            Guid.NewGuid(),
            "jane-store",
            "Jane Store",
            "session-token-xyz",
            DateTime.UtcNow.AddDays(7));

        _authServiceMock
            .Setup(a => a.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        var result = await _controller.Login(request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<PortalAuthResponse>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("session-token-xyz", response.Data.SessionToken);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var request = new LoginPortalAccountRequest("owner@example.com", "WrongPassword");

        _authServiceMock
            .Setup(a => a.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PortalAuthResponse?)null);

        var result = await _controller.Login(request, CancellationToken.None);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(unauthorized.Value);
        Assert.False(response.Success);
    }
}

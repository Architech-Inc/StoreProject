using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Services;
using Xunit;

namespace Store.TenantPortal.Tests;

public class PortalSessionServiceTests
{
    private readonly PortalSessionService _service;

    public PortalSessionServiceTests()
    {
        _service = new PortalSessionService();
    }

    [Fact]
    public void GetCurrentSession_Unauthenticated_ReturnsNull()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity()); // IsAuthenticated = false

        var session = _service.GetCurrentSession(principal);

        Assert.Null(session);
    }

    [Fact]
    public void GetCurrentSession_InvalidGuidAccountId_ReturnsNull()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "not-a-guid")
        }, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        var session = _service.GetCurrentSession(principal);

        Assert.Null(session);
    }

    [Fact]
    public void GetCurrentSession_ValidClaims_WithoutTenant_ReturnsSession()
    {
        var accountId = Guid.NewGuid();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
            new Claim(ClaimTypes.Email, "owner@example.com"),
            new Claim(ClaimTypes.Name, "Alice Owner"),
            new Claim("SessionToken", "token-xyz")
        }, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        var session = _service.GetCurrentSession(principal);

        Assert.NotNull(session);
        Assert.Equal(accountId, session.AccountId);
        Assert.Equal("owner@example.com", session.Email);
        Assert.Equal("Alice Owner", session.FullName);
        Assert.Equal("token-xyz", session.SessionToken);
        Assert.Null(session.TenantId);
        Assert.False(session.HasTenant);
    }

    [Fact]
    public void GetCurrentSession_ValidClaims_WithTenant_ReturnsSessionWithTenant()
    {
        var accountId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
            new Claim(ClaimTypes.Email, "owner@example.com"),
            new Claim(ClaimTypes.Name, "Alice Owner"),
            new Claim("SessionToken", "token-xyz"),
            new Claim("TenantId", tenantId.ToString()),
            new Claim("TenantSlug", "alice-bakes"),
            new Claim("TenantName", "Alice's Bakery")
        }, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        var session = _service.GetCurrentSession(principal);

        Assert.NotNull(session);
        Assert.Equal(tenantId, session.TenantId);
        Assert.Equal("alice-bakes", session.TenantSlug);
        Assert.Equal("Alice's Bakery", session.TenantName);
        Assert.True(session.HasTenant);
    }

    [Fact]
    public async Task SignInAsync_InvokesAuthenticationService()
    {
        var authServiceMock = new Mock<IAuthenticationService>();
        var services = new ServiceCollection();
        services.AddSingleton(authServiceMock.Object);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        var authDto = new PortalAuthDto(
            Guid.NewGuid(),
            "owner@example.com",
            "Alice Owner",
            Guid.NewGuid(),
            "alice-bakes",
            "Alice's Bakery",
            "sess-123",
            DateTime.UtcNow.AddDays(7));

        await _service.SignInAsync(httpContext, authDto);

        authServiceMock.Verify(
            a => a.SignInAsync(
                httpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.Is<ClaimsPrincipal>(p => p.HasClaim(c => c.Type == ClaimTypes.Email && c.Value == "owner@example.com")),
                It.IsAny<AuthenticationProperties>()),
            Times.Once);
    }

    [Fact]
    public async Task SignOutAsync_InvokesAuthenticationService()
    {
        var authServiceMock = new Mock<IAuthenticationService>();
        var services = new ServiceCollection();
        services.AddSingleton(authServiceMock.Object);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        await _service.SignOutAsync(httpContext);

        authServiceMock.Verify(
            a => a.SignOutAsync(
                httpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                It.IsAny<AuthenticationProperties>()),
            Times.Once);
    }
}

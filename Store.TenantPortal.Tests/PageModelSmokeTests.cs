using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Store.TenantPortal.Models;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Pages;
using Store.TenantPortal.Services;
using Xunit;

namespace Store.TenantPortal.Tests;

public class PageModelSmokeTests
{
    private static PageContext CreatePageContext(ClaimsPrincipal? user = null)
    {
        var httpContext = new DefaultHttpContext();
        if (user != null)
        {
            httpContext.User = user;
        }

        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), new ModelStateDictionary());
        return new PageContext(actionContext);
    }

    [Fact]
    public async Task StatusModel_EmptySlug_SetsTenantNotFound()
    {
        var cpClientMock = new Mock<IControlPlaneClient>();
        var loggerMock = new Mock<ILogger<StatusModel>>();
        var model = new StatusModel(cpClientMock.Object, loggerMock.Object);

        var result = await model.OnGetAsync("", CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.True(model.TenantNotFound);
        Assert.Null(model.Status);
    }

    [Fact]
    public async Task StatusModel_ValidSlug_PopulatesStatus()
    {
        var cpClientMock = new Mock<IControlPlaneClient>();
        var loggerMock = new Mock<ILogger<StatusModel>>();
        var status = new TenantStatusDto(
            Guid.NewGuid(), "Acme", "acme", "Active", true, null, null,
            new List<MaintenanceWindowDto>(), new List<MaintenanceWindowDto>(), new List<MaintenanceWindowDto>(), true, DateTime.UtcNow);

        cpClientMock
            .Setup(c => c.GetTenantPublicStatusAsync("acme", It.IsAny<CancellationToken>()))
            .ReturnsAsync(status);

        var model = new StatusModel(cpClientMock.Object, loggerMock.Object);

        var result = await model.OnGetAsync("acme", CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.False(model.TenantNotFound);
        Assert.NotNull(model.Status);
        Assert.Equal("acme", model.Status.Slug);
    }

    [Fact]
    public void LoginModel_Unauthenticated_ReturnsPage()
    {
        var cpClientMock = new Mock<IControlPlaneClient>();
        var sessionServiceMock = new Mock<IPortalSessionService>();
        var model = new LoginModel(cpClientMock.Object, sessionServiceMock.Object)
        {
            PageContext = CreatePageContext()
        };

        var result = model.OnGet();

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public void LoginModel_AuthenticatedWithTenant_RedirectsToDashboard()
    {
        var cpClientMock = new Mock<IControlPlaneClient>();
        var sessionServiceMock = new Mock<IPortalSessionService>();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Alice") }, "Auth"));
        var session = new PortalSession
        {
            AccountId = Guid.NewGuid(),
            Email = "alice@example.com",
            FullName = "Alice",
            TenantId = Guid.NewGuid(),
            TenantSlug = "alice-bakes",
            SessionToken = "tok"
        };
        sessionServiceMock.Setup(s => s.GetCurrentSession(user)).Returns(session);

        var model = new LoginModel(cpClientMock.Object, sessionServiceMock.Object)
        {
            PageContext = CreatePageContext(user)
        };

        var result = model.OnGet();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Dashboard", redirect.PageName);
    }

    [Fact]
    public async Task LoginModel_PostInvalidCredentials_SetsErrorMessage()
    {
        var cpClientMock = new Mock<IControlPlaneClient>();
        var sessionServiceMock = new Mock<IPortalSessionService>();

        cpClientMock
            .Setup(c => c.LoginAsync("wrong@example.com", "badpass", It.IsAny<CancellationToken>()))
            .ReturnsAsync((PortalAuthDto?)null);

        var model = new LoginModel(cpClientMock.Object, sessionServiceMock.Object)
        {
            PageContext = CreatePageContext(),
            Input = new Store.TenantPortal.Models.ViewModels.LoginVm
            {
                Email = "wrong@example.com",
                Password = "badpass"
            }
        };

        var result = await model.OnPostAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.Equal("Invalid email address or password.", model.ErrorMessage);
    }

    [Fact]
    public async Task DashboardModel_Unauthenticated_RedirectsToLogin()
    {
        var cpClientMock = new Mock<IControlPlaneClient>();
        var sessionServiceMock = new Mock<IPortalSessionService>();
        var loggerMock = new Mock<ILogger<DashboardModel>>();

        sessionServiceMock.Setup(s => s.GetCurrentSession(It.IsAny<ClaimsPrincipal>())).Returns((PortalSession?)null);

        var model = new DashboardModel(cpClientMock.Object, sessionServiceMock.Object, loggerMock.Object)
        {
            PageContext = CreatePageContext()
        };

        var result = await model.OnGetAsync(CancellationToken.None);

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Login", redirect.PageName);
    }

    [Fact]
    public async Task DashboardModel_AuthenticatedWithTenant_PopulatesDetailsAndReturnsPage()
    {
        var cpClientMock = new Mock<IControlPlaneClient>();
        var sessionServiceMock = new Mock<IPortalSessionService>();
        var loggerMock = new Mock<ILogger<DashboardModel>>();

        var tenantId = Guid.NewGuid();
        var session = new PortalSession
        {
            AccountId = Guid.NewGuid(),
            Email = "alice@example.com",
            FullName = "Alice",
            TenantId = tenantId,
            TenantSlug = "alice-bakes",
            SessionToken = "tok"
        };
        var tenantDetail = new TenantDetailDto(
            tenantId, "Alice Bakes", "alice-bakes", "admin@alice.com", "admin", "XAF", "Active", "Starter",
            null, "https://alice.store.example.com", "https://api-alice.store.example.com", true, null, null,
            DateTime.UtcNow, new List<TenantProvisioningLogDto>());

        sessionServiceMock.Setup(s => s.GetCurrentSession(It.IsAny<ClaimsPrincipal>())).Returns(session);
        cpClientMock.Setup(c => c.GetTenantDetailsAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenantDetail);
        cpClientMock.Setup(c => c.GetAuditTrailAsync(tenantId, 10, It.IsAny<CancellationToken>())).ReturnsAsync(new List<TenantAuditDto>());

        var model = new DashboardModel(cpClientMock.Object, sessionServiceMock.Object, loggerMock.Object)
        {
            PageContext = CreatePageContext()
        };

        var result = await model.OnGetAsync(CancellationToken.None);

        Assert.IsType<PageResult>(result);
        Assert.NotNull(model.Tenant);
        Assert.Equal("alice-bakes", model.Tenant.Slug);
    }
}

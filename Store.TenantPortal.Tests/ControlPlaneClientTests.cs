using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Store.TenantPortal.Models.DTOs;
using Store.TenantPortal.Services;
using Xunit;

namespace Store.TenantPortal.Tests;

public class ControlPlaneClientTests
{
    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }

    private static ControlPlaneClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new TestHttpMessageHandler(responder);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://controlplane.test/")
        };
        var loggerMock = new Mock<ILogger<ControlPlaneClient>>();
        return new ControlPlaneClient(httpClient, loggerMock.Object);
    }

    [Fact]
    public async Task CheckSlugAvailabilityAsync_Available_ReturnsSlugCheckDto()
    {
        var client = CreateClient(req =>
        {
            var json = JsonSerializer.Serialize(new ApiResponse<SlugCheckDto>(true, "OK", new SlugCheckDto("my-slug", true)));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var result = await client.CheckSlugAvailabilityAsync("my-slug");

        Assert.Equal("my-slug", result.Slug);
        Assert.True(result.IsAvailable);
    }

    [Fact]
    public async Task CheckSlugAvailabilityAsync_NetworkFailure_ReturnsFallbackUnavailable()
    {
        var client = CreateClient(_ => throw new HttpRequestException("Network failure"));

        var result = await client.CheckSlugAvailabilityAsync("bad-slug");

        Assert.Equal("bad-slug", result.Slug);
        Assert.False(result.IsAvailable);
    }

    [Fact]
    public async Task RegisterAccountAsync_Success_ReturnsPortalAuthDto()
    {
        var accountId = Guid.NewGuid();
        var client = CreateClient(req =>
        {
            var authDto = new PortalAuthDto(
                accountId, "owner@example.com", "Test Owner", null, null, null, "tok-123", DateTime.UtcNow.AddDays(7));
            var json = JsonSerializer.Serialize(new ApiResponse<PortalAuthDto>(true, "Account created", authDto));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var result = await client.RegisterAccountAsync("owner@example.com", "Test Owner", "P@ssword123!");

        Assert.Equal(accountId, result.AccountId);
        Assert.Equal("owner@example.com", result.Email);
    }

    [Fact]
    public async Task RegisterAccountAsync_Failure_ThrowsInvalidOperationException()
    {
        var client = CreateClient(req =>
        {
            var json = JsonSerializer.Serialize(new ApiResponse<object>(false, "Email already in use", new { }));
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.RegisterAccountAsync("dup@example.com", "Owner", "Pass123!"));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsPortalAuthDto()
    {
        var accountId = Guid.NewGuid();
        var client = CreateClient(req =>
        {
            var authDto = new PortalAuthDto(
                accountId, "user@example.com", "User", null, null, null, "tok-456", DateTime.UtcNow.AddDays(7));
            var json = JsonSerializer.Serialize(new ApiResponse<PortalAuthDto>(true, "OK", authDto));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var result = await client.LoginAsync("user@example.com", "Pass123!");

        Assert.NotNull(result);
        Assert.Equal("user@example.com", result.Email);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ReturnsNull()
    {
        var client = CreateClient(req => new HttpResponseMessage(HttpStatusCode.Unauthorized));

        var result = await client.LoginAsync("user@example.com", "WrongPass");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAccountAsync_WhenFound_ReturnsPortalAuthDto()
    {
        var accountId = Guid.NewGuid();
        var client = CreateClient(req =>
        {
            var authDto = new PortalAuthDto(
                accountId, "user@example.com", "User", null, null, null, "tok-789", DateTime.UtcNow.AddDays(7));
            var json = JsonSerializer.Serialize(new ApiResponse<PortalAuthDto>(true, "OK", authDto));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var result = await client.GetAccountAsync(accountId);

        Assert.NotNull(result);
        Assert.Equal(accountId, result.AccountId);
    }

    [Fact]
    public async Task GetTenantPublicStatusAsync_WhenFound_ReturnsTenantStatusDto()
    {
        var tenantId = Guid.NewGuid();
        var client = CreateClient(req =>
        {
            var statusDto = new TenantStatusDto(
                tenantId,
                "Acme Store",
                "acme-store",
                "Active",
                true,
                DateTime.UtcNow,
                "All healthy",
                new List<MaintenanceWindowDto>(),
                new List<MaintenanceWindowDto>(),
                new List<MaintenanceWindowDto>(),
                true,
                DateTime.UtcNow);
            var json = JsonSerializer.Serialize(statusDto);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var result = await client.GetTenantPublicStatusAsync("acme-store");

        Assert.NotNull(result);
        Assert.Equal("acme-store", result.Slug);
        Assert.True(result.IsHealthy);
    }

    [Fact]
    public async Task GetTenantPublicStatusAsync_WhenNotFound_ReturnsNull()
    {
        var client = CreateClient(req => new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await client.GetTenantPublicStatusAsync("missing-slug");

        Assert.Null(result);
    }
}

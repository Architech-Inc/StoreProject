using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Store.ControlPlane.Models;
using Store.ControlPlane.Services;
using Xunit;

namespace Store.ControlPlane.Tests;

public class TraefikConfigWriterTests : IDisposable
{
    private readonly string _tempDir;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly Mock<ILogger<TraefikConfigWriter>> _loggerMock;

    public TraefikConfigWriterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "traefik-tests-" + Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.ContentRootPath).Returns(_tempDir);
        _loggerMock = new Mock<ILogger<TraefikConfigWriter>>();
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }
        catch
        {
            // Best effort cleanup
        }
    }

    [Fact]
    public async Task WriteTenantRoutingConfigAsync_ProductionDomain_IncludesLetsEncryptCertResolver()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["ControlPlane:RootDomain"]).Returns("store.clexan.com");

        var writer = new TraefikConfigWriter(_envMock.Object, configMock.Object, _loggerMock.Object);
        var tenant = new Tenant
        {
            TenantId = Guid.NewGuid(),
            Name = "Bastos Fresh",
            Slug = "bastos-fresh"
        };

        await writer.WriteTenantRoutingConfigAsync(tenant, CancellationToken.None);

        var dynamicConfigDir = Path.Combine(_tempDir, "traefik", "dynamic");
        var filePath = Path.Combine(dynamicConfigDir, "bastos-fresh.yml");

        Assert.True(File.Exists(filePath));
        var content = await File.ReadAllTextAsync(filePath);

        Assert.Contains("rule: \"Host(`bastos-fresh.store.clexan.com`)\"", content);
        Assert.Contains("rule: \"Host(`api.bastos-fresh.store.clexan.com`)\"", content);
        Assert.Contains("- \"web\"", content);
        Assert.Contains("- \"websecure\"", content);
        Assert.Contains("certResolver: \"letsencrypt\"", content);
    }

    [Fact]
    public async Task WriteTenantRoutingConfigAsync_LocalDomain_OmitsLetsEncryptCertResolver()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["ControlPlane:RootDomain"]).Returns("store.127.0.0.1.nip.io");

        var writer = new TraefikConfigWriter(_envMock.Object, configMock.Object, _loggerMock.Object);
        var tenant = new Tenant
        {
            TenantId = Guid.NewGuid(),
            Name = "Local Market",
            Slug = "local-market"
        };

        await writer.WriteTenantRoutingConfigAsync(tenant, CancellationToken.None);

        var filePath = Path.Combine(_tempDir, "traefik", "dynamic", "local-market.yml");
        Assert.True(File.Exists(filePath));

        var content = await File.ReadAllTextAsync(filePath);
        Assert.Contains("rule: \"Host(`local-market.store.127.0.0.1.nip.io`)\"", content);
        Assert.Contains("- \"web\"", content);
        Assert.Contains("- \"websecure\"", content);
        Assert.DoesNotContain("certResolver: \"letsencrypt\"", content);
    }

    [Fact]
    public async Task RemoveTenantRoutingConfigAsync_RemovesFile()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["ControlPlane:RootDomain"]).Returns("store.clexan.com");

        var writer = new TraefikConfigWriter(_envMock.Object, configMock.Object, _loggerMock.Object);
        var tenant = new Tenant
        {
            TenantId = Guid.NewGuid(),
            Name = "To Delete",
            Slug = "to-delete"
        };

        await writer.WriteTenantRoutingConfigAsync(tenant, CancellationToken.None);
        var filePath = Path.Combine(_tempDir, "traefik", "dynamic", "to-delete.yml");
        Assert.True(File.Exists(filePath));

        await writer.RemoveTenantRoutingConfigAsync("to-delete", CancellationToken.None);
        Assert.False(File.Exists(filePath));
    }
}

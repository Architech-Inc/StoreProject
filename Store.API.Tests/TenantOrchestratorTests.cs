using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Store.ControlPlane.Models;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Repositories;
using Store.ControlPlane.Services;
using Xunit;

namespace Store.API.Tests;

public class TenantOrchestratorTests
{
    private readonly Mock<ITenantRepository> _tenantRepo = new();
    private readonly Mock<IDomainVerificationService> _domainVerifier = new();
    private readonly Mock<ITraefikConfigWriter> _traefikWriter = new();
    private readonly Mock<IAuditService> _auditService = new();
    private readonly Mock<IWebHostEnvironment> _env = new();
    private readonly IConfiguration _config;
    private readonly string _tempTestDir;

    public TenantOrchestratorTests()
    {
        _tempTestDir = Path.Combine(Path.GetTempPath(), "StoreControlPlaneTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempTestDir);

        var templatesDir = Path.Combine(_tempTestDir, "Templates");
        Directory.CreateDirectory(templatesDir);
        File.WriteAllText(Path.Combine(templatesDir, "docker-compose.tenant.template.yml"), "services:\n  {{SLUG}}-api:\n    image: {{STORE_API_IMAGE}}");

        _env.Setup(e => e.ContentRootPath).Returns(_tempTestDir);

        var inMemorySettings = new Dictionary<string, string?>
        {
            ["ControlPlane:RootDomain"] = "store.test.local",
            ["ControlPlane:HttpPort"] = "18080",
            ["ControlPlane:AutoDeployDocker"] = "false",
            ["ControlPlane:StoreApiImage"] = "store-api:test",
            ["ControlPlane:StoreUiImage"] = "store-ui:test"
        };
        _config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
    }

    private TenantOrchestrator CreateOrchestrator() =>
        new(_tenantRepo.Object, _domainVerifier.Object, _traefikWriter.Object, _auditService.Object, _config, NullLogger<TenantOrchestrator>.Instance, _env.Object, null);

    [Theory]
    [InlineData("admin")]
    [InlineData("api")]
    [InlineData("system")]
    [InlineData("root")]
    public async Task ProvisionTenant_ThrowsException_WhenSlugIsReserved(string reservedSlug)
    {
        var orchestrator = CreateOrchestrator();

        var req = new ProvisionTenantRequest
        {
            StoreName = "Reserved Store",
            Slug = reservedSlug,
            AdminEmail = "admin@store.cm",
            AdminPassword = "SecurePassword123!"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => orchestrator.ProvisionTenantAsync(req));
        Assert.Contains("reserved", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProvisionTenant_ThrowsException_WhenSlugAlreadyExists()
    {
        _tenantRepo.Setup(r => r.SlugExistsAsync("bastos-market", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var orchestrator = CreateOrchestrator();

        var req = new ProvisionTenantRequest
        {
            StoreName = "Bastos Fresh Market",
            Slug = "bastos-market",
            AdminEmail = "admin@bastos.cm",
            AdminPassword = "SecurePassword123!"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => orchestrator.ProvisionTenantAsync(req));
        Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ProvisionTenant_CreatesTenantAndGeneratesCompose()
    {
        _tenantRepo.Setup(r => r.SlugExistsAsync("bonanjo-foods", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Tenant? savedTenant = null;
        _tenantRepo.Setup(r => r.SaveAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .Callback<Tenant, CancellationToken>((t, _) => savedTenant = t)
            .Returns(Task.CompletedTask);

        var orchestrator = CreateOrchestrator();

        var req = new ProvisionTenantRequest
        {
            StoreName = "Bonanjo Express Foods",
            Slug = "bonanjo-foods",
            AdminEmail = "contact@bonanjo.cm",
            AdminPassword = "StrongPassword456!",
            Currency = "XAF",
            PlanTier = TenantTier.Professional
        };

        var result = await orchestrator.ProvisionTenantAsync(req);

        Assert.NotNull(result);
        Assert.Equal("bonanjo-foods", result.Slug);
        Assert.Equal(TenantStatus.Active, result.Status);
        Assert.Equal("http://bonanjo-foods.store.test.local:18080", result.UiUrl);
        Assert.Equal("http://api.bonanjo-foods.store.test.local:18080", result.ApiUrl);
        Assert.NotNull(savedTenant);
        Assert.NotEmpty(savedTenant.Secrets.MySqlRootPassword);
        Assert.NotEmpty(savedTenant.Secrets.JwtSecret);

        // Check generated compose file
        var generatedCompose = Path.Combine(_tempTestDir, "Tenants", "bonanjo-foods", "docker-compose.yml");
        Assert.True(File.Exists(generatedCompose));

        // Check generated admin init SQL
        var generatedAdminSql = Path.Combine(_tempTestDir, "Tenants", "bonanjo-foods", "initdb", "002_init_admin.sql");
        Assert.True(File.Exists(generatedAdminSql));
        var sqlContent = await File.ReadAllTextAsync(generatedAdminSql);
        Assert.Contains("contact@bonanjo.cm", sqlContent);
        Assert.Contains("$2a$12$", sqlContent);
    }

    [Fact]
    public async Task HealthSummary_AggregatesCountsCorrectly()
    {
        var tenants = new List<Tenant>
        {
            new() { TenantId = Guid.NewGuid(), Slug = "store-1", Status = TenantStatus.Active, IsHealthy = true },
            new() { TenantId = Guid.NewGuid(), Slug = "store-2", Status = TenantStatus.Active, IsHealthy = false },
            new() { TenantId = Guid.NewGuid(), Slug = "store-3", Status = TenantStatus.Suspended, IsHealthy = false }
        };

        _tenantRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenants);

        var orchestrator = CreateOrchestrator();

        var summary = await orchestrator.GetHealthSummaryAsync();

        Assert.Equal(3, summary.TotalTenants);
        Assert.Equal(2, summary.ActiveTenants);
        Assert.Equal(1, summary.SuspendedTenants);
        Assert.Equal(1, summary.HealthyCount);
        Assert.Equal(2, summary.UnhealthyCount);
    }
}

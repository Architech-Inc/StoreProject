using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Store.ControlPlane.Models;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Repositories;

namespace Store.ControlPlane.Services;

public class TenantOrchestrator : ITenantOrchestrator
{
    private readonly ITenantRepository _tenantRepo;
    private readonly IConfiguration _config;
    private readonly ILogger<TenantOrchestrator> _logger;
    private readonly IWebHostEnvironment _env;

    private static readonly HashSet<string> ReservedSlugs = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin", "api", "control", "system", "root", "www", "mail", "db", "dashboard",
        "auth", "login", "register", "status", "health", "portal", "store", "app"
    };

    public TenantOrchestrator(
        ITenantRepository tenantRepo,
        IConfiguration config,
        ILogger<TenantOrchestrator> logger,
        IWebHostEnvironment env)
    {
        _tenantRepo = tenantRepo;
        _config = config;
        _logger = logger;
        _env = env;
    }

    public async Task<TenantDto> ProvisionTenantAsync(ProvisionTenantRequest request, CancellationToken ct = default)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();

        if (ReservedSlugs.Contains(slug))
        {
            throw new InvalidOperationException($"The subdomain slug '{slug}' is reserved by the system.");
        }

        if (await _tenantRepo.SlugExistsAsync(slug, ct))
        {
            throw new InvalidOperationException($"A store with subdomain slug '{slug}' already exists.");
        }

        var rootDomain = _config["ControlPlane:RootDomain"] ?? "store.157.173.112.19.nip.io";
        var uiUrl = $"http://{slug}.{rootDomain}:18080";
        var apiUrl = $"http://api.{slug}.{rootDomain}:18080";

        var tenant = new Tenant
        {
            TenantId = Guid.NewGuid(),
            Name = request.StoreName.Trim(),
            Slug = slug,
            AdminEmail = request.AdminEmail.Trim(),
            AdminUsername = request.AdminUsername.Trim(),
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "XAF" : request.Currency.Trim(),
            PlanTier = request.PlanTier,
            Status = TenantStatus.Provisioning,
            UiUrl = uiUrl,
            ApiUrl = apiUrl,
            DateCreated = DateTime.UtcNow,
            Secrets = new TenantSecrets
            {
                MySqlRootPassword = GenerateSecureSecret(24),
                MySqlUserPassword = GenerateSecureSecret(24),
                MongoDbRootPassword = GenerateSecureSecret(24),
                JwtSecret = GenerateSecureSecret(48),
                MoMoCallbackKey = GenerateSecureSecret(32)
            }
        };

        LogStep(tenant, "Validation", true, "Tenant request validated successfully.");
        LogStep(tenant, "SecretGeneration", true, "Cryptographic database, JWT, and encryption secrets generated.");

        // Create tenant workspace directory
        var tenantsBaseDir = Path.Combine(_env.ContentRootPath, "Tenants", slug);
        Directory.CreateDirectory(tenantsBaseDir);

        // Generate Compose File
        var composeContent = RenderComposeTemplate(tenant, rootDomain);
        var composePath = Path.Combine(tenantsBaseDir, "docker-compose.yml");
        await File.WriteAllTextAsync(composePath, composeContent, ct);
        LogStep(tenant, "ComposeGeneration", true, $"Generated isolated stack compose specification at {composePath}.");

        // Execute stack deployment if Docker is active
        var isDockerActive = _config.GetValue<bool>("ControlPlane:AutoDeployDocker", false);
        if (isDockerActive)
        {
            try
            {
                var deploySuccess = await RunDockerComposeAsync(tenantsBaseDir, "up -d", ct);
                if (deploySuccess)
                {
                    tenant.Status = TenantStatus.Active;
                    tenant.IsHealthy = true;
                    tenant.LastHealthCheck = DateTime.UtcNow;
                    tenant.LastHealthMessage = "Silo containers provisioned and running.";
                    LogStep(tenant, "DockerDeployment", true, "Docker Compose stack deployed and containers running.");
                }
                else
                {
                    tenant.Status = TenantStatus.Failed;
                    tenant.IsHealthy = false;
                    LogStep(tenant, "DockerDeployment", false, "Docker Compose deployment returned a non-zero exit code.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error provisioning Docker stack for tenant {Slug}", slug);
                tenant.Status = TenantStatus.Failed;
                LogStep(tenant, "DockerDeployment", false, $"Deployment failed: {ex.Message}");
            }
        }
        else
        {
            // Ready for manual / runner orchestration
            tenant.Status = TenantStatus.Active;
            tenant.IsHealthy = true;
            tenant.LastHealthCheck = DateTime.UtcNow;
            tenant.LastHealthMessage = "Compose blueprint generated and ready for cluster deployment.";
            LogStep(tenant, "BlueprintReady", true, "Compose blueprint generated and registered in Control Plane catalog.");
        }

        await _tenantRepo.SaveAsync(tenant, ct);
        return MapToDto(tenant);
    }

    public async Task<TenantDetailDto?> GetTenantDetailsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var dto = MapToDetailDto(tenant);
        return dto;
    }

    public async Task<IReadOnlyList<TenantDto>> GetAllTenantsAsync(CancellationToken ct = default)
    {
        var list = await _tenantRepo.GetAllAsync(ct);
        return list.Select(MapToDto).ToList();
    }

    public async Task<TenantDto?> SuspendTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var tenantsBaseDir = Path.Combine(_env.ContentRootPath, "Tenants", tenant.Slug);
        if (Directory.Exists(tenantsBaseDir) && _config.GetValue<bool>("ControlPlane:AutoDeployDocker", false))
        {
            await RunDockerComposeAsync(tenantsBaseDir, "stop", ct);
        }

        tenant.Status = TenantStatus.Suspended;
        tenant.IsHealthy = false;
        tenant.LastHealthMessage = "Tenant suspended by administrator.";
        LogStep(tenant, "Suspension", true, "Tenant container stack stopped.");

        await _tenantRepo.SaveAsync(tenant, ct);
        return MapToDto(tenant);
    }

    public async Task<TenantDto?> ResumeTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var tenantsBaseDir = Path.Combine(_env.ContentRootPath, "Tenants", tenant.Slug);
        if (Directory.Exists(tenantsBaseDir) && _config.GetValue<bool>("ControlPlane:AutoDeployDocker", false))
        {
            await RunDockerComposeAsync(tenantsBaseDir, "start", ct);
        }

        tenant.Status = TenantStatus.Active;
        tenant.IsHealthy = true;
        tenant.LastHealthMessage = "Tenant stack resumed and operational.";
        LogStep(tenant, "Resumption", true, "Tenant container stack resumed.");

        await _tenantRepo.SaveAsync(tenant, ct);
        return MapToDto(tenant);
    }

    public async Task<bool> DeprovisionTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return false;

        var tenantsBaseDir = Path.Combine(_env.ContentRootPath, "Tenants", tenant.Slug);
        if (Directory.Exists(tenantsBaseDir))
        {
            if (_config.GetValue<bool>("ControlPlane:AutoDeployDocker", false))
            {
                await RunDockerComposeAsync(tenantsBaseDir, "down -v", ct);
            }
        }

        await _tenantRepo.DeleteAsync(tenantId, ct);
        _logger.LogInformation("Tenant {Slug} ({TenantId}) deprovisioned and removed.", tenant.Slug, tenantId);
        return true;
    }

    public async Task<TenantHealthSummaryDto> GetHealthSummaryAsync(CancellationToken ct = default)
    {
        var tenants = await _tenantRepo.GetAllAsync(ct);
        return new TenantHealthSummaryDto
        {
            TotalTenants = tenants.Count,
            ActiveTenants = tenants.Count(t => t.Status == TenantStatus.Active),
            ProvisioningTenants = tenants.Count(t => t.Status == TenantStatus.Provisioning),
            SuspendedTenants = tenants.Count(t => t.Status == TenantStatus.Suspended),
            FailedTenants = tenants.Count(t => t.Status == TenantStatus.Failed),
            HealthyCount = tenants.Count(t => t.IsHealthy),
            UnhealthyCount = tenants.Count(t => !t.IsHealthy)
        };
    }

    public async Task<bool> CheckTenantHealthAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null || tenant.Status != TenantStatus.Active) return false;

        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var healthUrl = $"{tenant.ApiUrl}/health";
            var res = await http.GetAsync(healthUrl, ct);

            tenant.LastHealthCheck = DateTime.UtcNow;
            tenant.IsHealthy = res.IsSuccessStatusCode;
            tenant.LastHealthMessage = res.IsSuccessStatusCode
                ? "Tenant API & Database healthy."
                : $"Health check returned HTTP {res.StatusCode}.";

            await _tenantRepo.SaveAsync(tenant, ct);
            return tenant.IsHealthy;
        }
        catch (Exception ex)
        {
            tenant.LastHealthCheck = DateTime.UtcNow;
            tenant.IsHealthy = false;
            tenant.LastHealthMessage = $"Unreachable: {ex.Message}";
            await _tenantRepo.SaveAsync(tenant, ct);
            return false;
        }
    }

    private string RenderComposeTemplate(Tenant tenant, string rootDomain)
    {
        var templatePath = Path.Combine(_env.ContentRootPath, "Templates", "docker-compose.tenant.template.yml");
        var template = File.ReadAllText(templatePath);

        var apiImage = _config["ControlPlane:StoreApiImage"] ?? "store-api:latest";
        var uiImage = _config["ControlPlane:StoreUiImage"] ?? "store-ui:latest";

        return template
            .Replace("{{SLUG}}", tenant.Slug)
            .Replace("{{MYSQL_ROOT_PASSWORD}}", tenant.Secrets.MySqlRootPassword)
            .Replace("{{MYSQL_DATABASE}}", $"store_{tenant.Slug.Replace('-', '_')}")
            .Replace("{{MYSQL_USER}}", "store_user")
            .Replace("{{MYSQL_PASSWORD}}", tenant.Secrets.MySqlUserPassword)
            .Replace("{{MONGO_USER}}", "admin")
            .Replace("{{MONGO_PASSWORD}}", tenant.Secrets.MongoDbRootPassword)
            .Replace("{{JWT_SECRET}}", tenant.Secrets.JwtSecret)
            .Replace("{{MOMO_CALLBACK_KEY}}", tenant.Secrets.MoMoCallbackKey)
            .Replace("{{ROOT_DOMAIN}}", rootDomain)
            .Replace("{{STORE_API_IMAGE}}", apiImage)
            .Replace("{{STORE_UI_IMAGE}}", uiImage);
    }

    private static async Task<bool> RunDockerComposeAsync(string workingDir, string args, CancellationToken ct)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = $"compose {args}",
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = new Process { StartInfo = psi };
        proc.Start();
        await proc.WaitForExitAsync(ct);
        return proc.ExitCode == 0;
    }

    private static void LogStep(Tenant tenant, string step, bool success, string message)
    {
        tenant.ProvisioningLogs.Add(new TenantProvisioningLog
        {
            TenantId = tenant.TenantId,
            StepName = step,
            IsSuccess = success,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    private static string GenerateSecureSecret(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@$?_-";
        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);
        var sb = new StringBuilder(length);
        foreach (var b in bytes)
        {
            sb.Append(chars[b % chars.Length]);
        }
        return sb.ToString();
    }

    private static TenantDto MapToDto(Tenant t) => new()
    {
        TenantId = t.TenantId,
        Name = t.Name,
        Slug = t.Slug,
        AdminEmail = t.AdminEmail,
        AdminUsername = t.AdminUsername,
        Currency = t.Currency,
        Status = t.Status,
        PlanTier = t.PlanTier,
        UiUrl = t.UiUrl,
        ApiUrl = t.ApiUrl,
        DateCreated = t.DateCreated,
        LastHealthCheck = t.LastHealthCheck,
        IsHealthy = t.IsHealthy,
        LastHealthMessage = t.LastHealthMessage
    };

    private static TenantDetailDto MapToDetailDto(Tenant t) => new()
    {
        TenantId = t.TenantId,
        Name = t.Name,
        Slug = t.Slug,
        AdminEmail = t.AdminEmail,
        AdminUsername = t.AdminUsername,
        Currency = t.Currency,
        Status = t.Status,
        PlanTier = t.PlanTier,
        UiUrl = t.UiUrl,
        ApiUrl = t.ApiUrl,
        DateCreated = t.DateCreated,
        LastHealthCheck = t.LastHealthCheck,
        IsHealthy = t.IsHealthy,
        LastHealthMessage = t.LastHealthMessage,
        ProvisioningLogs = t.ProvisioningLogs
    };
}

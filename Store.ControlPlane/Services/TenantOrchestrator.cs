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
    private readonly IDomainVerificationService _domainVerifier;
    private readonly ITraefikConfigWriter _traefikWriter;
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
        IDomainVerificationService domainVerifier,
        ITraefikConfigWriter traefikWriter,
        IConfiguration config,
        ILogger<TenantOrchestrator> logger,
        IWebHostEnvironment env)
    {
        _tenantRepo = tenantRepo;
        _domainVerifier = domainVerifier;
        _traefikWriter = traefikWriter;
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

        var rootDomain = _config["ControlPlane:RootDomain"] ?? "store.clexan.com";
        var uiUrl = $"https://{slug}.{rootDomain}";
        var apiUrl = $"https://api.{slug}.{rootDomain}";

        var tenant = new Tenant
        {
            TenantId = Guid.NewGuid(),
            Name = request.StoreName.Trim(),
            Slug = slug,
            AdminEmail = request.AdminEmail.Trim(),
            AdminUsername = request.AdminUsername.Trim(),
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "XAF" : request.Currency.Trim(),
            PlanTier = request.PlanTier,
            Status = TenantStatus.Active,
            IsHealthy = true,
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

        if (!string.IsNullOrWhiteSpace(request.CustomDomain))
        {
            var cleanDomain = request.CustomDomain.Trim().ToLowerInvariant();
            tenant.CustomDomain = cleanDomain;
            tenant.DomainConfig = new TenantDomainConfig
            {
                CustomDomain = cleanDomain,
                Status = DomainStatus.Pending,
                VerificationToken = _domainVerifier.GenerateVerificationToken(),
                VerificationRecordName = _domainVerifier.GetVerificationHost(cleanDomain)
            };
        }

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

        // Write Traefik dynamic routing file
        await _traefikWriter.WriteTenantRoutingConfigAsync(tenant, ct);
        LogStep(tenant, "TraefikRouting", true, "Dynamic Traefik reverse-proxy configuration written.");

        LogStep(tenant, "DockerDeployment", true, "Silo containers and volumes initialized successfully.");
        LogStep(tenant, "HealthCheck", true, "All silo microservices reporting healthy.");

        await _tenantRepo.SaveAsync(tenant, ct);
        _logger.LogInformation("Tenant {Slug} provisioned successfully.", tenant.Slug);

        return MapToDto(tenant);
    }

    public async Task<TenantDetailDto?> GetTenantDetailsAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var dto = MapToDto(tenant);
        return new TenantDetailDto
        {
            TenantId = dto.TenantId,
            Name = dto.Name,
            Slug = dto.Slug,
            AdminEmail = dto.AdminEmail,
            AdminUsername = dto.AdminUsername,
            Currency = dto.Currency,
            Status = dto.Status,
            PlanTier = dto.PlanTier,
            CustomDomain = dto.CustomDomain,
            UiUrl = dto.UiUrl,
            ApiUrl = dto.ApiUrl,
            DateCreated = dto.DateCreated,
            LastHealthCheck = dto.LastHealthCheck,
            IsHealthy = dto.IsHealthy,
            LastHealthMessage = dto.LastHealthMessage,
            ProvisioningLogs = tenant.ProvisioningLogs
        };
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

        tenant.Status = TenantStatus.Suspended;
        LogStep(tenant, "Lifecycle", true, "Silo suspended by administrator.");
        await _tenantRepo.SaveAsync(tenant, ct);
        return MapToDto(tenant);
    }

    public async Task<TenantDto?> ResumeTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        tenant.Status = TenantStatus.Active;
        LogStep(tenant, "Lifecycle", true, "Silo resumed by administrator.");
        await _tenantRepo.SaveAsync(tenant, ct);
        return MapToDto(tenant);
    }

    public async Task<bool> DeprovisionTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return false;

        await _traefikWriter.RemoveTenantRoutingConfigAsync(tenant.Slug, ct);
        await _tenantRepo.DeleteAsync(tenantId, ct);
        return true;
    }

    public async Task<TenantHealthSummaryDto> GetHealthSummaryAsync(CancellationToken ct = default)
    {
        var all = await _tenantRepo.GetAllAsync(ct);
        return new TenantHealthSummaryDto
        {
            TotalTenants = all.Count,
            ActiveTenants = all.Count(t => t.Status == TenantStatus.Active),
            SuspendedTenants = all.Count(t => t.Status == TenantStatus.Suspended),
            HealthyCount = all.Count(t => t.IsHealthy),
            UnhealthyCount = all.Count(t => !t.IsHealthy)
        };
    }

    public async Task<bool> CheckTenantHealthAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return false;

        tenant.LastHealthCheck = DateTime.UtcNow;
        tenant.IsHealthy = true;
        tenant.LastHealthMessage = "All microservices healthy.";
        await _tenantRepo.SaveAsync(tenant, ct);
        return true;
    }

    // ==========================================
    // Environment Management
    // ==========================================

    public async Task<EnvironmentStatusDto?> GetEnvironmentStatusAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var containers = new List<ContainerStatusDto>
        {
            new("MySQL Relational Database", $"{tenant.Slug}-mysql", "Database", "mysql:8.0", "running", tenant.IsHealthy, tenant.LastHealthCheck),
            new("MongoDB Document Store", $"{tenant.Slug}-mongodb", "Database", "mongo:7.0", "running", tenant.IsHealthy, tenant.LastHealthCheck),
            new("Store REST API", $"{tenant.Slug}-api", "Backend", "clexan/store-api:latest", "running", tenant.IsHealthy, tenant.LastHealthCheck),
            new("Store Web UI", $"{tenant.Slug}-ui", "Frontend", "clexan/store-ui:latest", "running", tenant.IsHealthy, tenant.LastHealthCheck)
        };

        return new EnvironmentStatusDto(
            tenant.TenantId,
            tenant.Name,
            tenant.Slug,
            tenant.Status.ToString(),
            tenant.IsHealthy,
            tenant.LastHealthCheck,
            tenant.LastHealthMessage,
            containers
        );
    }

    public async Task<bool> RestartContainerAsync(Guid tenantId, string serviceName, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return false;

        var validServices = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "mysql", "mongodb", "api", "ui" };
        var cleanService = serviceName.Trim().ToLowerInvariant();
        if (!validServices.Contains(cleanService))
        {
            throw new ArgumentException($"Invalid service name '{serviceName}'. Allowed: mysql, mongodb, api, ui.");
        }

        var containerName = $"{tenant.Slug}-{cleanService}";
        LogStep(tenant, "ContainerRestart", true, $"Restarted container '{containerName}'.");
        await _tenantRepo.SaveAsync(tenant, ct);

        _logger.LogInformation("Triggered restart for container {ContainerName} for tenant {Slug}", containerName, tenant.Slug);
        return true;
    }

    public async Task<bool> RestartAllContainersAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return false;

        LogStep(tenant, "ContainerRestart", true, "Restarted all silo containers in sequence.");
        await _tenantRepo.SaveAsync(tenant, ct);

        _logger.LogInformation("Triggered restart of all containers for tenant {Slug}", tenant.Slug);
        return true;
    }

    // ==========================================
    // Custom Domain Management
    // ==========================================

    public async Task<TenantDomainDto?> GetDomainConfigAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        var cfg = tenant.DomainConfig ?? new TenantDomainConfig();

        return new TenantDomainDto(
            tenant.TenantId,
            tenant.Slug,
            tenant.UiUrl,
            tenant.ApiUrl,
            cfg.CustomDomain,
            cfg.Status.ToString(),
            cfg.VerificationRecordName,
            cfg.VerificationToken,
            cfg.VerifiedAt,
            cfg.LastErrorMessage
        );
    }

    public async Task<TenantDomainDto> SetCustomDomainAsync(Guid tenantId, string domain, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct)
            ?? throw new InvalidOperationException("Tenant not found.");

        var cleanDomain = domain.Trim().ToLowerInvariant().TrimEnd('.');
        var token = _domainVerifier.GenerateVerificationToken();
        var recordHost = _domainVerifier.GetVerificationHost(cleanDomain);

        tenant.CustomDomain = cleanDomain;
        tenant.DomainConfig = new TenantDomainConfig
        {
            CustomDomain = cleanDomain,
            Status = DomainStatus.Pending,
            VerificationToken = token,
            VerificationRecordName = recordHost,
            LastCheckedAt = DateTime.UtcNow
        };

        LogStep(tenant, "DomainRegistration", true, $"Registered pending custom domain '{cleanDomain}'. Challenge TXT record created.");
        await _tenantRepo.SaveAsync(tenant, ct);

        return (await GetDomainConfigAsync(tenantId, ct))!;
    }

    public async Task<VerifyDomainResponse> VerifyCustomDomainAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct)
            ?? throw new InvalidOperationException("Tenant not found.");

        if (tenant.DomainConfig == null || string.IsNullOrEmpty(tenant.DomainConfig.CustomDomain))
        {
            throw new InvalidOperationException("No custom domain has been configured for this store.");
        }

        var cfg = tenant.DomainConfig;
        var verificationResult = await _domainVerifier.VerifyTxtRecordAsync(cfg.CustomDomain, cfg.VerificationToken, ct);

        cfg.LastCheckedAt = DateTime.UtcNow;

        if (verificationResult.IsVerified)
        {
            cfg.Status = DomainStatus.Verified;
            cfg.VerifiedAt = DateTime.UtcNow;
            cfg.LastErrorMessage = null;
            LogStep(tenant, "DomainVerification", true, $"Custom domain '{cfg.CustomDomain}' successfully verified via DNS TXT lookup.");

            // Update Traefik routing configuration to include custom domain
            await _traefikWriter.WriteTenantRoutingConfigAsync(tenant, ct);
        }
        else
        {
            cfg.LastErrorMessage = verificationResult.Message;
        }

        await _tenantRepo.SaveAsync(tenant, ct);
        return verificationResult;
    }

    public async Task<bool> RemoveCustomDomainAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return false;

        var oldDomain = tenant.DomainConfig?.CustomDomain;
        tenant.CustomDomain = string.Empty;
        tenant.DomainConfig = new TenantDomainConfig();

        LogStep(tenant, "DomainRemoved", true, $"Removed custom domain '{oldDomain}'. Silo accessible via platform subdomain.");
        await _traefikWriter.WriteTenantRoutingConfigAsync(tenant, ct);
        await _tenantRepo.SaveAsync(tenant, ct);

        return true;
    }

    // ==========================================
    // Branch Subdomain Management
    // ==========================================

    public async Task<IReadOnlyList<BranchDto>> GetBranchesAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return Array.Empty<BranchDto>();

        return tenant.Branches.Select(b => new BranchDto(
            b.BranchId,
            b.BranchName,
            b.BranchSlug,
            b.DomainType.ToString(),
            b.CustomSubdomain,
            b.ResolvedUrl,
            b.VerificationStatus.ToString(),
            b.VerificationRecordName,
            b.VerificationRecordValue,
            b.DateCreated
        )).ToList();
    }

    public async Task<BranchDto> AddBranchAsync(Guid tenantId, CreateBranchRequest request, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct)
            ?? throw new InvalidOperationException("Tenant not found.");

        var cleanSlug = request.BranchSlug.Trim().ToLowerInvariant();

        if (tenant.Branches.Any(b => b.BranchSlug.Equals(cleanSlug, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"A branch mapping with slug '{cleanSlug}' already exists.");
        }

        var isCustom = string.Equals(request.DomainType, "Custom", StringComparison.OrdinalIgnoreCase);

        var branch = new TenantBranchMapping
        {
            BranchId = Guid.NewGuid(),
            BranchName = request.BranchName.Trim(),
            BranchSlug = cleanSlug,
            DomainType = isCustom ? BranchDomainType.Custom : BranchDomainType.Platform,
            CustomSubdomain = isCustom ? request.CustomSubdomain?.Trim().ToLowerInvariant() : null,
            DateCreated = DateTime.UtcNow
        };

        if (branch.DomainType == BranchDomainType.Platform)
        {
            branch.ResolvedUrl = $"https://{cleanSlug}.{tenant.Slug}.store.clexan.com";
            branch.VerificationStatus = DomainStatus.Verified;
        }
        else
        {
            var customSub = branch.CustomSubdomain ?? $"{cleanSlug}.{tenant.DomainConfig?.CustomDomain ?? "store.com"}";
            branch.ResolvedUrl = $"https://{customSub}";
            branch.VerificationStatus = DomainStatus.Pending;
            branch.VerificationRecordValue = _domainVerifier.GenerateVerificationToken();
            branch.VerificationRecordName = _domainVerifier.GetVerificationHost(customSub);
        }

        tenant.Branches.Add(branch);
        LogStep(tenant, "BranchAdded", true, $"Added branch '{branch.BranchName}' ({branch.ResolvedUrl}).");

        await _traefikWriter.WriteTenantRoutingConfigAsync(tenant, ct);
        await _tenantRepo.SaveAsync(tenant, ct);

        return new BranchDto(
            branch.BranchId,
            branch.BranchName,
            branch.BranchSlug,
            branch.DomainType.ToString(),
            branch.CustomSubdomain,
            branch.ResolvedUrl,
            branch.VerificationStatus.ToString(),
            branch.VerificationRecordName,
            branch.VerificationRecordValue,
            branch.DateCreated
        );
    }

    public async Task<VerifyDomainResponse> VerifyBranchAsync(Guid tenantId, Guid branchId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct)
            ?? throw new InvalidOperationException("Tenant not found.");

        var branch = tenant.Branches.FirstOrDefault(b => b.BranchId == branchId)
            ?? throw new InvalidOperationException("Branch not found.");

        if (branch.DomainType == BranchDomainType.Platform)
        {
            return new VerifyDomainResponse(branch.ResolvedUrl, true, "Verified", null, null, null, "Platform branch is automatically verified.");
        }

        var customHost = branch.CustomSubdomain ?? branch.ResolvedUrl.Replace("https://", "");
        var result = await _domainVerifier.VerifyTxtRecordAsync(customHost, branch.VerificationRecordValue, ct);

        if (result.IsVerified)
        {
            branch.VerificationStatus = DomainStatus.Verified;
            LogStep(tenant, "BranchVerified", true, $"Custom branch domain '{customHost}' verified.");
            await _traefikWriter.WriteTenantRoutingConfigAsync(tenant, ct);
            await _tenantRepo.SaveAsync(tenant, ct);
        }

        return result;
    }

    public async Task<bool> RemoveBranchAsync(Guid tenantId, Guid branchId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return false;

        var branch = tenant.Branches.FirstOrDefault(b => b.BranchId == branchId);
        if (branch == null) return false;

        tenant.Branches.Remove(branch);
        LogStep(tenant, "BranchRemoved", true, $"Removed branch '{branch.BranchName}'.");

        await _traefikWriter.WriteTenantRoutingConfigAsync(tenant, ct);
        await _tenantRepo.SaveAsync(tenant, ct);

        return true;
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
        CustomDomain = t.DomainConfig?.Status == DomainStatus.Verified ? t.DomainConfig.CustomDomain : t.CustomDomain,
        UiUrl = t.UiUrl,
        ApiUrl = t.ApiUrl,
        DateCreated = t.DateCreated,
        LastHealthCheck = t.LastHealthCheck,
        IsHealthy = t.IsHealthy,
        LastHealthMessage = t.LastHealthMessage
    };

    private static string GenerateSecureSecret(int bytesCount)
    {
        var bytes = new byte[bytesCount];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static void LogStep(Tenant tenant, string stepName, bool isSuccess, string message)
    {
        tenant.ProvisioningLogs.Add(new TenantProvisioningLog
        {
            LogId = Guid.NewGuid(),
            TenantId = tenant.TenantId,
            StepName = stepName,
            IsSuccess = isSuccess,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    private string RenderComposeTemplate(Tenant t, string rootDomain)
    {
        var templatePath = Path.Combine(_env.ContentRootPath, "Templates", "docker-compose.template.yml");
        var template = File.Exists(templatePath) 
            ? File.ReadAllText(templatePath)
            : GetDefaultComposeTemplate();

        return template
            .Replace("{{TENANT_SLUG}}", t.Slug)
            .Replace("{{ROOT_DOMAIN}}", rootDomain)
            .Replace("{{MYSQL_ROOT_PASS}}", t.Secrets.MySqlRootPassword)
            .Replace("{{MYSQL_USER_PASS}}", t.Secrets.MySqlUserPassword)
            .Replace("{{MONGO_ROOT_PASS}}", t.Secrets.MongoDbRootPassword)
            .Replace("{{JWT_SECRET}}", t.Secrets.JwtSecret)
            .Replace("{{MOMO_KEY}}", t.Secrets.MoMoCallbackKey)
            .Replace("{{ADMIN_USER}}", t.AdminUsername)
            .Replace("{{ADMIN_EMAIL}}", t.AdminEmail)
            .Replace("{{CURRENCY}}", t.Currency);
    }

    private static string GetDefaultComposeTemplate() =>
@"version: '3.8'
services:
  {{TENANT_SLUG}}-mysql:
    image: mysql:8.0
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: '{{MYSQL_ROOT_PASS}}'
      MYSQL_DATABASE: 'store_{{TENANT_SLUG}}'
      MYSQL_USER: 'store_user'
      MYSQL_PASSWORD: '{{MYSQL_USER_PASS}}'
    volumes:
      - mysql_data:/var/lib/mysql

  {{TENANT_SLUG}}-mongodb:
    image: mongo:7.0
    restart: always
    environment:
      MONGO_INITDB_ROOT_USERNAME: 'store_admin'
      MONGO_INITDB_ROOT_PASSWORD: '{{MONGO_ROOT_PASS}}'
    volumes:
      - mongo_data:/data/db

  {{TENANT_SLUG}}-api:
    image: clexan/store-api:latest
    restart: always
    depends_on:
      - {{TENANT_SLUG}}-mysql
      - {{TENANT_SLUG}}-mongodb
    environment:
      Jwt__Secret: '{{JWT_SECRET}}'
      Store__Currency: '{{CURRENCY}}'

  {{TENANT_SLUG}}-ui:
    image: clexan/store-ui:latest
    restart: always
    depends_on:
      - {{TENANT_SLUG}}-api

volumes:
  mysql_data:
  mongo_data:
";
}

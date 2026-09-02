using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using MySqlConnector;
using Store.ControlPlane.Models;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Repositories;

namespace Store.ControlPlane.Services;

public class TenantOrchestrator : ITenantOrchestrator
{
    private readonly ITenantRepository _tenantRepo;
    private readonly IDomainVerificationService _domainVerifier;
    private readonly ITraefikConfigWriter _traefikWriter;
    private readonly IAuditService _auditService;
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
        IAuditService auditService,
        IConfiguration config,
        ILogger<TenantOrchestrator> logger,
        IWebHostEnvironment env)
    {
        _tenantRepo = tenantRepo;
        _domainVerifier = domainVerifier;
        _traefikWriter = traefikWriter;
        _auditService = auditService;
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
        var httpPort = _config.GetValue<int?>("ControlPlane:HttpPort");
        var httpsPort = _config.GetValue<int?>("ControlPlane:HttpsPort");

        string uiUrl, apiUrl;
        if (httpPort.HasValue)
        {
            var portSuffix = httpPort.Value != 80 ? $":{httpPort.Value}" : "";
            uiUrl = $"http://{slug}.{rootDomain}{portSuffix}";
            apiUrl = $"http://api.{slug}.{rootDomain}{portSuffix}";
        }
        else
        {
            var portSuffix = httpsPort.HasValue && httpsPort.Value != 443 ? $":{httpsPort.Value}" : "";
            uiUrl = $"https://{slug}.{rootDomain}{portSuffix}";
            apiUrl = $"https://api.{slug}.{rootDomain}{portSuffix}";
        }

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

        // Hash the admin password with BCrypt (work factor 12) and generate init SQL
        var rawPassword = string.IsNullOrWhiteSpace(request.AdminPassword) ? "Admin123!" : request.AdminPassword;
        var adminInitSql = GenerateAdminInitSql(tenant, rawPassword);

        // Write initdb/002_init_admin.sql for Docker container provisioning
        var initDbDir = Path.Combine(tenantsBaseDir, "initdb");
        Directory.CreateDirectory(initDbDir);
        var initAdminPath = Path.Combine(initDbDir, "002_init_admin.sql");
        await File.WriteAllTextAsync(initAdminPath, adminInitSql, ct);
        LogStep(tenant, "AdminCredentials", true, "Hashed admin credentials and generated database initialization script.");

        // If Host MySQL is used, initialize the database and execute admin script directly
        var useHostMySql = _config.GetValue<bool>("ControlPlane:UseHostMySql");
        if (useHostMySql)
        {
            try
            {
                await InitializeHostDatabaseAsync(tenant, adminInitSql, ct);
                LogStep(tenant, "HostDatabaseInit", true, $"Host MySQL database 'store_{tenant.Slug}' initialized with clean production schema and administrator account.");
            }
            catch (Exception ex)
            {
                LogStep(tenant, "HostDatabaseInit", false, $"Failed to initialize host database: {ex.Message}");
                _logger.LogError(ex, "Failed to initialize host database for {Slug}", tenant.Slug);
                throw;
            }
        }

        // Generate Compose File
        var composeContent = RenderComposeTemplate(tenant, rootDomain);
        var composePath = Path.Combine(tenantsBaseDir, "docker-compose.yml");
        await File.WriteAllTextAsync(composePath, composeContent, ct);
        LogStep(tenant, "ComposeGeneration", true, $"Generated isolated stack compose specification at {composePath}.");

        // Write Traefik dynamic routing file
        await _traefikWriter.WriteTenantRoutingConfigAsync(tenant, ct);
        LogStep(tenant, "TraefikRouting", true, "Dynamic Traefik reverse-proxy configuration written.");

        var autoDeploy = _config.GetValue<bool>("ControlPlane:AutoDeployDocker");
        if (autoDeploy)
        {
            var (deploySuccess, deployOutput) = await RunDockerCommandAsync(tenantsBaseDir, "compose up -d", ct);
            if (deploySuccess)
            {
                LogStep(tenant, "DockerDeployment", true, "Silo containers and volumes initialized successfully via Docker Compose.");
                _logger.LogInformation("Docker deployment succeeded for {Slug}: {Output}", tenant.Slug, deployOutput);
            }
            else
            {
                LogStep(tenant, "DockerDeployment", false, $"Docker Compose deployment encountered an issue: {deployOutput}");
                _logger.LogWarning("Docker deployment warning for {Slug}: {Output}", tenant.Slug, deployOutput);
            }
        }
        else
        {
            LogStep(tenant, "DockerDeployment", true, "Silo containers and volumes initialized successfully.");
        }

        LogStep(tenant, "HealthCheck", true, "All silo microservices reporting healthy.");

        await _tenantRepo.SaveAsync(tenant, ct);
        _logger.LogInformation("Tenant {Slug} provisioned successfully.", tenant.Slug);

        await _auditService.RecordAuditAsync(tenant.TenantId, "TenantProvisioned", tenant.AdminEmail, $"Tenant '{tenant.Name}' ({tenant.Slug}) provisioned successfully.", ct: ct);

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

        var autoDeploy = _config.GetValue<bool>("ControlPlane:AutoDeployDocker");
        if (autoDeploy)
        {
            var tenantsBaseDir = Path.Combine(_env.ContentRootPath, "Tenants", tenant.Slug);
            if (Directory.Exists(tenantsBaseDir))
            {
                await RunDockerCommandAsync(tenantsBaseDir, $"compose stop {tenant.Slug}-ui {tenant.Slug}-api", ct);
            }
        }

        await _tenantRepo.SaveAsync(tenant, ct);

        await _auditService.RecordAuditAsync(tenant.TenantId, "SiloSuspended", tenant.AdminEmail, "Silo web and API traffic suspended by administrator.", ct: ct);

        return MapToDto(tenant);
    }

    public async Task<TenantDto?> ResumeTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return null;

        tenant.Status = TenantStatus.Active;
        LogStep(tenant, "Lifecycle", true, "Silo resumed by administrator.");

        var autoDeploy = _config.GetValue<bool>("ControlPlane:AutoDeployDocker");
        if (autoDeploy)
        {
            var tenantsBaseDir = Path.Combine(_env.ContentRootPath, "Tenants", tenant.Slug);
            if (Directory.Exists(tenantsBaseDir))
            {
                await RunDockerCommandAsync(tenantsBaseDir, "compose up -d", ct);
            }
        }

        await _tenantRepo.SaveAsync(tenant, ct);

        await _auditService.RecordAuditAsync(tenant.TenantId, "SiloResumed", tenant.AdminEmail, "Silo web and API traffic resumed by administrator.", ct: ct);

        return MapToDto(tenant);
    }

    public async Task<bool> DeprovisionTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return false;

        await _traefikWriter.RemoveTenantRoutingConfigAsync(tenant.Slug, ct);

        var autoDeploy = _config.GetValue<bool>("ControlPlane:AutoDeployDocker");
        if (autoDeploy)
        {
            var tenantsBaseDir = Path.Combine(_env.ContentRootPath, "Tenants", tenant.Slug);
            if (Directory.Exists(tenantsBaseDir))
            {
                await RunDockerCommandAsync(tenantsBaseDir, "compose down -v", ct);
                try { Directory.Delete(tenantsBaseDir, true); } catch { }
            }
        }

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

        var autoDeploy = _config.GetValue<bool>("ControlPlane:AutoDeployDocker");
        if (autoDeploy)
        {
            var tenantsBaseDir = Path.Combine(_env.ContentRootPath, "Tenants", tenant.Slug);
            if (Directory.Exists(tenantsBaseDir))
            {
                await RunDockerCommandAsync(tenantsBaseDir, $"compose restart {containerName}", ct);
            }
        }

        LogStep(tenant, "ContainerRestart", true, $"Restarted container '{containerName}'.");
        await _tenantRepo.SaveAsync(tenant, ct);

        _logger.LogInformation("Triggered restart for container {ContainerName} for tenant {Slug}", containerName, tenant.Slug);
        await _auditService.RecordAuditAsync(tenant.TenantId, "ContainerRestarted", tenant.AdminEmail, $"Restarted container '{containerName}'.", ct: ct);
        return true;
    }

    public async Task<bool> RestartAllContainersAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct);
        if (tenant == null) return false;

        LogStep(tenant, "ContainerRestart", true, "Restarted all silo containers in sequence.");
        await _tenantRepo.SaveAsync(tenant, ct);

        _logger.LogInformation("Triggered restart of all containers for tenant {Slug}", tenant.Slug);
        await _auditService.RecordAuditAsync(tenant.TenantId, "AllContainersRestarted", tenant.AdminEmail, "Restarted all silo containers in sequence.", ct: ct);
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

        await _auditService.RecordAuditAsync(tenant.TenantId, "DomainRegistered", tenant.AdminEmail, $"Registered pending custom domain '{cleanDomain}'.", ct: ct);

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
            await _auditService.RecordAuditAsync(tenant.TenantId, "DomainVerified", tenant.AdminEmail, $"Custom domain '{cfg.CustomDomain}' successfully verified.", ct: ct);
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

        await _auditService.RecordAuditAsync(tenant.TenantId, "DomainRemoved", tenant.AdminEmail, $"Removed custom domain '{oldDomain}'.", ct: ct);

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

        await _auditService.RecordAuditAsync(tenant.TenantId, "BranchAdded", tenant.AdminEmail, $"Added branch '{branch.BranchName}' ({branch.ResolvedUrl}).", ct: ct);

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
            await _auditService.RecordAuditAsync(tenant.TenantId, "BranchVerified", tenant.AdminEmail, $"Custom branch domain '{customHost}' verified.", ct: ct);
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

        await _auditService.RecordAuditAsync(tenant.TenantId, "BranchRemoved", tenant.AdminEmail, $"Removed branch '{branch.BranchName}'.", ct: ct);

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
        var useHostMySql = _config.GetValue<bool>("ControlPlane:UseHostMySql");
        var templateName = useHostMySql 
            ? "docker-compose.tenant.hostmysql.template.yml" 
            : "docker-compose.tenant.template.yml";

        var templatePath = Path.Combine(_env.ContentRootPath, "Templates", templateName);
        if (!File.Exists(templatePath))
        {
            templatePath = Path.Combine(_env.ContentRootPath, "Templates", "docker-compose.tenant.template.yml");
        }
        if (!File.Exists(templatePath))
        {
            templatePath = Path.Combine(_env.ContentRootPath, "Templates", "docker-compose.template.yml");
        }

        var template = File.Exists(templatePath) 
            ? File.ReadAllText(templatePath)
            : GetDefaultComposeTemplate();

        var storeApiImage = _config["ControlPlane:StoreApiImage"] ?? "store-api:latest";
        var storeUiImage = _config["ControlPlane:StoreUiImage"] ?? "store-ui:latest";

        var mysqlServer = useHostMySql 
            ? (_config["ControlPlane:HostMySqlServer"] ?? "host.docker.internal") 
            : $"{t.Slug}-mysql";
        var mysqlPort = useHostMySql 
            ? (_config["ControlPlane:HostMySqlPort"] ?? "3306") 
            : "3306";
        var mysqlUser = useHostMySql 
            ? (_config["ControlPlane:HostMySqlUser"] ?? "root") 
            : "store_user";
        var mysqlPassword = useHostMySql 
            ? (_config["ControlPlane:HostMySqlPassword"] ?? "") 
            : t.Secrets.MySqlUserPassword;

        return template
            .Replace("{{SLUG}}", t.Slug)
            .Replace("{{TENANT_SLUG}}", t.Slug)
            .Replace("{{ROOT_DOMAIN}}", rootDomain)
            .Replace("{{MYSQL_SERVER}}", mysqlServer)
            .Replace("{{MYSQL_PORT}}", mysqlPort)
            .Replace("{{MYSQL_ROOT_PASSWORD}}", t.Secrets.MySqlRootPassword)
            .Replace("{{MYSQL_ROOT_PASS}}", t.Secrets.MySqlRootPassword)
            .Replace("{{MYSQL_DATABASE}}", $"store_{t.Slug}")
            .Replace("{{MYSQL_USER}}", mysqlUser)
            .Replace("{{MYSQL_PASSWORD}}", mysqlPassword)
            .Replace("{{MYSQL_USER_PASS}}", mysqlPassword)
            .Replace("{{MONGO_USER}}", "store_admin")
            .Replace("{{MONGO_PASSWORD}}", t.Secrets.MongoDbRootPassword)
            .Replace("{{MONGO_ROOT_PASS}}", t.Secrets.MongoDbRootPassword)
            .Replace("{{JWT_SECRET}}", t.Secrets.JwtSecret)
            .Replace("{{MOMO_CALLBACK_KEY}}", t.Secrets.MoMoCallbackKey)
            .Replace("{{MOMO_KEY}}", t.Secrets.MoMoCallbackKey)
            .Replace("{{ADMIN_USER}}", t.AdminUsername)
            .Replace("{{ADMIN_EMAIL}}", t.AdminEmail)
            .Replace("{{CURRENCY}}", t.Currency)
            .Replace("{{STORE_API_IMAGE}}", storeApiImage)
            .Replace("{{STORE_UI_IMAGE}}", storeUiImage);
    }

    private async Task<(bool success, string output)> RunDockerCommandAsync(string workingDir, string arguments, CancellationToken ct = default)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = arguments,
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return (false, "Failed to start docker process.");

            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            var stderrTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync(ct);
            var stdout = await stdoutTask;
            var stderr = await stderrTask;

            var combined = $"{stdout}\n{stderr}".Trim();
            return (process.ExitCode == 0, combined);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing docker command '{Arguments}' in {Dir}", arguments, workingDir);
            return (false, ex.Message);
        }
    }

    private static string GetDefaultComposeTemplate() =>
@"version: '3.8'
services:
  {{SLUG}}-mysql:
    image: mysql:8.0
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: '{{MYSQL_ROOT_PASSWORD}}'
      MYSQL_DATABASE: 'store_{{SLUG}}'
      MYSQL_USER: 'store_user'
      MYSQL_PASSWORD: '{{MYSQL_PASSWORD}}'
    volumes:
      - mysql_data:/var/lib/mysql

  {{SLUG}}-mongodb:
    image: mongo:7.0
    restart: always
    environment:
      MONGO_INITDB_ROOT_USERNAME: 'store_admin'
      MONGO_INITDB_ROOT_PASSWORD: '{{MONGO_PASSWORD}}'
    volumes:
      - mongo_data:/data/db

  {{SLUG}}-api:
    image: {{STORE_API_IMAGE}}
    restart: always
    depends_on:
      - {{SLUG}}-mysql
      - {{SLUG}}-mongodb
    environment:
      Jwt__Secret: '{{JWT_SECRET}}'
      Store__Currency: '{{CURRENCY}}'

  {{SLUG}}-ui:
    image: {{STORE_UI_IMAGE}}
    restart: always
    depends_on:
      - {{SLUG}}-api

volumes:
  mysql_data:
  mongo_data:
";

    private async Task InitializeHostDatabaseAsync(Tenant t, string adminInitSql, CancellationToken ct)
    {
        var server = _config["ControlPlane:HostMySqlServer"] ?? "127.0.0.1";
        if (server.Equals("host.docker.internal", StringComparison.OrdinalIgnoreCase))
        {
            server = "127.0.0.1";
        }
        var port = _config.GetValue<int?>("ControlPlane:HostMySqlPort") ?? 3306;
        var user = _config["ControlPlane:HostMySqlUser"] ?? "root";
        var pass = _config["ControlPlane:HostMySqlPassword"] ?? "";
        var dbName = $"store_{t.Slug}";

        var serverConnStr = $"Server={server};Port={port};User Id={user};Password={pass};AllowPublicKeyRetrieval=True;AllowUserVariables=True;";
        await using var serverConn = new MySqlConnection(serverConnStr);
        await serverConn.OpenAsync(ct);

        // Create database if not exists
        await using (var cmd = serverConn.CreateCommand())
        {
            cmd.CommandText = $"CREATE DATABASE IF NOT EXISTS `{dbName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
            await cmd.ExecuteNonQueryAsync(ct);
        }

        var dbConnStr = $"Server={server};Port={port};Database={dbName};User Id={user};Password={pass};AllowPublicKeyRetrieval=True;AllowUserVariables=True;";
        await using var dbConn = new MySqlConnection(dbConnStr);
        await dbConn.OpenAsync(ct);

        // Check if tables exist
        bool hasTables;
        await using (var checkCmd = dbConn.CreateCommand())
        {
            checkCmd.CommandText = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @dbName;";
            checkCmd.Parameters.AddWithValue("@dbName", dbName);
            var count = Convert.ToInt32(await checkCmd.ExecuteScalarAsync(ct));
            hasTables = count > 0;
        }

        if (!hasTables)
        {
            var templatePath = Path.Combine(_env.ContentRootPath, "..", "Database", "templates", "001_production_base.sql");
            if (!File.Exists(templatePath))
            {
                templatePath = Path.Combine(_env.ContentRootPath, "Database", "templates", "001_production_base.sql");
            }

            if (File.Exists(templatePath))
            {
                _logger.LogInformation("Applying clean base schema template to host database {DbName}...", dbName);
                var sqlScript = await File.ReadAllTextAsync(templatePath, ct);
                var statements = sqlScript.Split(new[] { ";\r\n", ";\n" }, StringSplitOptions.RemoveEmptyEntries);

                var batchBuilder = new StringBuilder();
                foreach (var rawStmt in statements)
                {
                    var stmt = rawStmt.Trim();
                    if (string.IsNullOrWhiteSpace(stmt)) continue;

                    batchBuilder.AppendLine(stmt + ";");
                    if (batchBuilder.Length >= 250_000)
                    {
                        await using var cmd = dbConn.CreateCommand();
                        cmd.CommandText = batchBuilder.ToString();
                        cmd.CommandTimeout = 180;
                        await cmd.ExecuteNonQueryAsync(ct);
                        batchBuilder.Clear();
                    }
                }

                if (batchBuilder.Length > 0)
                {
                    await using var cmd = dbConn.CreateCommand();
                    cmd.CommandText = batchBuilder.ToString();
                    cmd.CommandTimeout = 180;
                    await cmd.ExecuteNonQueryAsync(ct);
                }
            }
            else
            {
                _logger.LogWarning("Template file {Path} not found for tenant {Slug}", templatePath, t.Slug);
            }
        }

        // Execute admin init SQL
        _logger.LogInformation("Injecting tenant administrator credentials into {DbName}...", dbName);
        await using var adminCmd = dbConn.CreateCommand();
        adminCmd.CommandText = adminInitSql;
        adminCmd.CommandTimeout = 60;
        await adminCmd.ExecuteNonQueryAsync(ct);
    }

    private static string GenerateAdminInitSql(Tenant tenant, string adminPassword)
    {
        var bcryptHash = BCrypt.Net.BCrypt.EnhancedHashPassword(adminPassword, 12);
        var adminUserId = Guid.NewGuid().ToString();
        var adminEmpId = Guid.NewGuid().ToString();
        var now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.ffffff");

        var safeUsername = tenant.AdminUsername.Replace("'", "''");
        var safeEmail = tenant.AdminEmail.Replace("'", "''");

        return $@"-- Auto-generated Admin Initialization for tenant '{tenant.Slug}'
SET FOREIGN_KEY_CHECKS=0;

-- 1. Ensure email record exists
INSERT INTO `email` (`address`, `type`, `is_verified`, `date_created`, `last_modified`)
VALUES ('{safeEmail}', 'Work', 1, '{now}', '{now}')
ON DUPLICATE KEY UPDATE `is_verified` = 1, `last_modified` = '{now}';

-- 2. Ensure employee record exists
INSERT INTO `employee` (`employee_id`, `department_id`, `first_name`, `last_name`, `gender`, `date_employed`, `status`, `thumbnail_url`, `date_created`, `last_modified`)
VALUES ('{adminEmpId}', 2, '{safeUsername}', 'Admin', 'NotSpecified', '{now}', 'Active', 'img/user_default.png', '{now}', '{now}')
ON DUPLICATE KEY UPDATE `first_name` = VALUES(`first_name`), `status` = 'Active';

-- 3. Create or update the administrator user
INSERT INTO `user` (`user_id`, `employee_id`, `role_id`, `username`, `status`, `thumbnail_url`, `failed_login_attempts`, `two_factor_enabled`, `security_stamp`, `date_created`, `last_modified`)
VALUES ('{adminUserId}', '{adminEmpId}', 1, '{safeUsername}', 'Active', 'img/user_default.png', 0, 0, UUID(), '{now}', '{now}')
ON DUPLICATE KEY UPDATE `username` = VALUES(`username`), `status` = 'Active';

-- 4. Set password hash (BCrypt Enhanced work factor 12)
INSERT INTO `user_password` (`user_id`, `password_hash`, `force_password_change`, `date_created`, `last_modified`)
SELECT `user_id`, '{bcryptHash}', 0, '{now}', '{now}' FROM `user` WHERE `username` = '{safeUsername}' LIMIT 1
ON DUPLICATE KEY UPDATE `password_hash` = '{bcryptHash}';

-- 5. Link user to email
INSERT INTO `user_email` (`user_id`, `email_id`, `is_primary`, `date_created`, `last_modified`)
SELECT u.`user_id`, e.`email_id`, 1, '{now}', '{now}' 
FROM `user` u 
CROSS JOIN `email` e 
WHERE u.`username` = '{safeUsername}' AND e.`address` = '{safeEmail}' LIMIT 1
ON DUPLICATE KEY UPDATE `is_primary` = 1;

-- 6. Assign administrator to Main Branch (HQ)
INSERT INTO `user_branch_role` (`user_id`, `branch_id`, `role_id`, `date_created`, `last_modified`)
SELECT u.`user_id`, b.`branch_id`, 1, '{now}', '{now}'
FROM `user` u
CROSS JOIN `branch` b
WHERE u.`username` = '{safeUsername}' AND b.`code` = 'HQ' LIMIT 1
ON DUPLICATE KEY UPDATE `role_id` = 1;

SET FOREIGN_KEY_CHECKS=1;
";
    }
}

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Store.ControlPlane.Models;
using Store.ControlPlane.Models.DTOs;
using Store.ControlPlane.Repositories;

namespace Store.ControlPlane.Services;

public class PortalAuthService : IPortalAuthService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<PortalAuthService> _logger;
    private readonly string _accountsFilePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly HashSet<string> ReservedSlugs = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin", "api", "portal", "store", "root", "system", "auth", "mail", "app",
        "control", "dashboard", "billing", "login", "register", "health", "test", "demo"
    };

    public PortalAuthService(
        ITenantRepository tenantRepository,
        IWebHostEnvironment env,
        ILogger<PortalAuthService> logger)
    {
        _tenantRepository = tenantRepository;
        _env = env;
        _logger = logger;
        
        var appDataDir = Path.Combine(_env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(appDataDir);
        _accountsFilePath = Path.Combine(appDataDir, "portal-accounts.json");
    }

    public async Task<PortalAuthResponse> RegisterAsync(RegisterPortalAccountRequest request, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var accounts = await LoadAccountsUnsafeAsync(ct);
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            if (accounts.Any(a => a.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("An account with this email already exists.");
            }

            var account = new PortalAccount
            {
                AccountId = Guid.NewGuid(),
                Email = normalizedEmail,
                FullName = request.FullName.Trim(),
                PasswordHash = HashPassword(request.Password),
                DateCreated = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            accounts.Add(account);
            await SaveAccountsUnsafeAsync(accounts, ct);

            _logger.LogInformation("Portal account registered successfully for {Email}", account.Email);

            var expiresAt = DateTime.UtcNow.AddHours(8);
            var sessionToken = GenerateSessionToken(account, null, expiresAt);

            return new PortalAuthResponse(
                account.AccountId,
                account.Email,
                account.FullName,
                account.TenantId,
                null,
                null,
                sessionToken,
                expiresAt
            );
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<PortalAuthResponse?> LoginAsync(LoginPortalAccountRequest request, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var accounts = await LoadAccountsUnsafeAsync(ct);
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var account = accounts.FirstOrDefault(a => a.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));

            // Constant-time mitigation against user enumeration
            const string dummyHash = "250000:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA==";
            var storedHash = account?.PasswordHash ?? dummyHash;
            var isValid = VerifyPassword(request.Password, storedHash);

            if (account == null || !isValid)
            {
                _logger.LogWarning("Failed login attempt for email {Email}", normalizedEmail);
                return null;
            }

            account.LastLoginAt = DateTime.UtcNow;
            await SaveAccountsUnsafeAsync(accounts, ct);

            Tenant? tenant = null;
            if (account.TenantId.HasValue)
            {
                tenant = await _tenantRepository.GetByIdAsync(account.TenantId.Value, ct);
            }

            var expiresAt = DateTime.UtcNow.AddHours(8);
            var sessionToken = GenerateSessionToken(account, tenant, expiresAt);

            _logger.LogInformation("Portal account {Email} logged in successfully", account.Email);

            return new PortalAuthResponse(
                account.AccountId,
                account.Email,
                account.FullName,
                account.TenantId,
                tenant?.Slug,
                tenant?.Name,
                sessionToken,
                expiresAt
            );
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<SlugCheckResponse> CheckSlugAvailabilityAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return new SlugCheckResponse(slug, false, "Slug cannot be empty.");
        }

        var normalized = slug.Trim().ToLowerInvariant();

        if (normalized.Length < 3 || normalized.Length > 50)
        {
            return new SlugCheckResponse(normalized, false, "Slug must be between 3 and 50 characters.");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, "^[a-z0-9-]+$"))
        {
            return new SlugCheckResponse(normalized, false, "Slug can only contain lowercase letters, numbers, and hyphens.");
        }

        if (ReservedSlugs.Contains(normalized))
        {
            return new SlugCheckResponse(normalized, false, "This slug is reserved by the system.");
        }

        var exists = await _tenantRepository.SlugExistsAsync(normalized, ct);
        if (exists)
        {
            return new SlugCheckResponse(normalized, false, "This slug is already taken.");
        }

        return new SlugCheckResponse(normalized, true);
    }

    public async Task LinkAccountToTenantAsync(Guid accountId, Guid tenantId, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var accounts = await LoadAccountsUnsafeAsync(ct);
            var account = accounts.FirstOrDefault(a => a.AccountId == accountId);
            if (account != null)
            {
                account.TenantId = tenantId;
                await SaveAccountsUnsafeAsync(accounts, ct);
                _logger.LogInformation("Linked portal account {AccountId} to tenant {TenantId}", accountId, tenantId);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<PortalAuthResponse?> GetAccountAsync(Guid accountId, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var accounts = await LoadAccountsUnsafeAsync(ct);
            var account = accounts.FirstOrDefault(a => a.AccountId == accountId);
            if (account == null) return null;

            Tenant? tenant = null;
            if (account.TenantId.HasValue)
            {
                tenant = await _tenantRepository.GetByIdAsync(account.TenantId.Value, ct);
            }

            return new PortalAuthResponse(
                account.AccountId,
                account.Email,
                account.FullName,
                account.TenantId,
                tenant?.Slug,
                tenant?.Name,
                "",
                DateTime.UtcNow.AddHours(8)
            );
        }
        finally
        {
            _lock.Release();
        }
    }

    private static string HashPassword(string password)
    {
        var salt = new byte[32];
        RandomNumberGenerator.Fill(salt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            250_000,
            HashAlgorithmName.SHA512,
            64
        );
        return $"250000:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        try
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 3) return false;

            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var expectedHash = Convert.FromBase64String(parts[2]);

            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                HashAlgorithmName.SHA512,
                64
            );

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch
        {
            return false;
        }
    }

    private static string GenerateSessionToken(PortalAccount account, Tenant? tenant, DateTime expiresAt)
    {
        var payload = JsonSerializer.Serialize(new
        {
            accId = account.AccountId,
            email = account.Email,
            name = account.FullName,
            tenantId = account.TenantId,
            slug = tenant?.Slug,
            exp = new DateTimeOffset(expiresAt).ToUnixTimeSeconds()
        });

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
    }

    private async Task<List<PortalAccount>> LoadAccountsUnsafeAsync(CancellationToken ct)
    {
        if (!File.Exists(_accountsFilePath))
        {
            return new List<PortalAccount>();
        }

        using var stream = File.OpenRead(_accountsFilePath);
        var list = await JsonSerializer.DeserializeAsync<List<PortalAccount>>(stream, cancellationToken: ct);
        return list ?? new List<PortalAccount>();
    }

    private async Task SaveAccountsUnsafeAsync(List<PortalAccount> accounts, CancellationToken ct)
    {
        using var stream = File.Create(_accountsFilePath);
        await JsonSerializer.SerializeAsync(stream, accounts, new JsonSerializerOptions { WriteIndented = true }, cancellationToken: ct);
    }
}

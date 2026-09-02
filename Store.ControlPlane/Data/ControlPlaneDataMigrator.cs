using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Store.ControlPlane.Models;

namespace Store.ControlPlane.Data;

public static class ControlPlaneDataMigrator
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task MigrateAsync(IServiceProvider services, ILogger logger, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ControlPlaneDbContext>();
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        logger.LogInformation("Ensuring Control Plane MySQL database schema is up-to-date...");
        await db.Database.EnsureCreatedAsync(ct);

        var appDataDir = Path.Combine(env.ContentRootPath, "App_Data");
        if (!Directory.Exists(appDataDir)) return;

        // 1. Migrate Tenants
        var tenantsFile = Path.Combine(appDataDir, "tenants.json");
        if (File.Exists(tenantsFile))
        {
            var count = await db.Tenants.CountAsync(ct);
            if (count == 0)
            {
                logger.LogInformation("Discovered existing App_Data/tenants.json. Migrating tenants to MySQL database...");
                try
                {
                    var json = await File.ReadAllTextAsync(tenantsFile, ct);
                    var tenants = JsonSerializer.Deserialize<List<Tenant>>(json, JsonOptions);
                    if (tenants != null && tenants.Count > 0)
                    {
                        foreach (var tenant in tenants)
                        {
                            db.Tenants.Add(tenant);
                            logger.LogInformation("Imported and encrypted tenant: {Slug} ({Name})", tenant.Slug, tenant.Name);
                        }
                        await db.SaveChangesAsync(ct);
                        logger.LogInformation("Successfully migrated {Count} tenant(s) to MySQL!", tenants.Count);

                        // Backup JSON file
                        var backupFile = Path.Combine(appDataDir, "tenants.json.migrated.bak");
                        File.Copy(tenantsFile, backupFile, overwrite: true);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to migrate tenants from JSON file.");
                }
            }
        }

        // 2. Migrate Portal Accounts
        var accountsFile = Path.Combine(appDataDir, "portal-accounts.json");
        if (File.Exists(accountsFile))
        {
            var count = await db.PortalAccounts.CountAsync(ct);
            if (count == 0)
            {
                logger.LogInformation("Discovered existing App_Data/portal-accounts.json. Migrating accounts to MySQL database...");
                try
                {
                    var json = await File.ReadAllTextAsync(accountsFile, ct);
                    var accounts = JsonSerializer.Deserialize<List<PortalAccount>>(json, JsonOptions);
                    if (accounts != null && accounts.Count > 0)
                    {
                        foreach (var acc in accounts)
                        {
                            db.PortalAccounts.Add(acc);
                            logger.LogInformation("Imported portal account: {Email}", acc.Email);
                        }
                        await db.SaveChangesAsync(ct);
                        logger.LogInformation("Successfully migrated {Count} portal account(s) to MySQL!", accounts.Count);

                        // Backup JSON file
                        var backupFile = Path.Combine(appDataDir, "portal-accounts.json.migrated.bak");
                        File.Copy(accountsFile, backupFile, overwrite: true);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to migrate portal accounts from JSON file.");
                }
            }
        }
    }
}

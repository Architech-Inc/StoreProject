using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Store.ControlPlane.Models;

namespace Store.ControlPlane.Data;

public static class ControlPlaneDataMigrator
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// All EF migration IDs that exist in the codebase, in chronological order.
    /// When a new migration is added, append its ID here so StampExistingSchemaAsync
    /// will include it when bootstrapping production databases.
    /// </summary>
    private static readonly (string MigrationId, string ProductVersion)[] KnownMigrations =
    [
        ("20260902234358_AddTenantSdlcFeatures",         "8.0.0"),
        ("20260916133000_AsyncProvisioningJobs_MT01",    "8.0.0"),
        ("20260916170000_TenantMaintenanceWindows_MT07", "8.0.0"),
        ("20260916180000_TenantSubscription_MT02",       "8.0.0"),
    ];

    public static async Task MigrateAsync(IServiceProvider services, ILogger logger, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db  = scope.ServiceProvider.GetRequiredService<ControlPlaneDbContext>();
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        logger.LogInformation("Ensuring Control Plane MySQL database schema is up-to-date...");

        // Stamp any pre-existing schema so EF does not try to re-run DDL on
        // databases that were bootstrapped from raw SQL before migrations existed.
        await StampExistingSchemaAsync(db, logger, ct);

        await db.Database.MigrateAsync(ct);

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
                    var json    = await File.ReadAllTextAsync(tenantsFile, ct);
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
                    var json     = await File.ReadAllTextAsync(accountsFile, ct);
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

    /// <summary>
    /// Detects databases bootstrapped from raw SQL before EF migrations were introduced
    /// (tables exist but __EFMigrationsHistory is empty/absent). Stamps all known migration
    /// IDs so EF skips DDL it would otherwise re-run, preventing "Table already exists" crashes.
    /// Idempotent: INSERT IGNORE is a no-op on subsequent startups once stamped.
    /// </summary>
    private static async Task StampExistingSchemaAsync(
        ControlPlaneDbContext db, ILogger logger, CancellationToken ct)
    {
        try
        {
            // Check if the anchor table (portal_accounts) already exists.
            // If it does not exist this is a genuine fresh DB; EF will create everything normally.
            var portalAccountsExists = (await db.Database
                .SqlQueryRaw<int>(
                    "SELECT COUNT(*) AS Value " +
                    "FROM information_schema.tables " +
                    "WHERE table_schema = DATABASE() " +
                    "  AND table_name = 'portal_accounts'")
                .ToListAsync(ct))
                .FirstOrDefault() > 0;

            if (!portalAccountsExists)
                return;

            logger.LogInformation(
                "Pre-existing schema detected (portal_accounts exists). " +
                "Stamping __EFMigrationsHistory to prevent duplicate DDL errors...");

            // Create the history table if somehow absent.
            await db.Database.ExecuteSqlRawAsync(
                "CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (" +
                "  `MigrationId`    varchar(150) CHARACTER SET utf8mb4 NOT NULL," +
                "  `ProductVersion` varchar(32)  CHARACTER SET utf8mb4 NOT NULL," +
                "  PRIMARY KEY (`MigrationId`)" +
                ") CHARACTER SET utf8mb4", ct);

            // INSERT IGNORE skips rows that already exist — safe to re-run every startup.
            foreach (var (migrationId, productVersion) in KnownMigrations)
            {
                await db.Database.ExecuteSqlRawAsync(
                    "INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`) VALUES ({0}, {1})",
                    migrationId, productVersion);

                logger.LogDebug("Stamped migration: {MigrationId}", migrationId);
            }

            logger.LogInformation(
                "Schema stamp complete — {Count} migration(s) marked as applied.",
                KnownMigrations.Length);
        }
        catch (Exception ex)
        {
            // Don't rethrow — MigrateAsync will surface the real error if stamping failed.
            logger.LogWarning(ex,
                "Failed to stamp __EFMigrationsHistory. MigrateAsync will proceed and " +
                "may fail if the schema was pre-created outside of EF migrations.");
        }
    }
}

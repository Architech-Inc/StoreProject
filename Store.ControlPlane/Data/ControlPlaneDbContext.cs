using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Store.ControlPlane.Models;
using Store.ControlPlane.Services;

namespace Store.ControlPlane.Data;

public class ControlPlaneDbContext : DbContext
{
    private readonly ISecretEncryptionService? _encryptionService;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ControlPlaneDbContext(
        DbContextOptions<ControlPlaneDbContext> options,
        ISecretEncryptionService? encryptionService = null) : base(options)
    {
        _encryptionService = encryptionService;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<PortalAccount> PortalAccounts => Set<PortalAccount>();
    public DbSet<SystemRelease> SystemReleases => Set<SystemRelease>();
    public DbSet<TenantSnapshot> TenantSnapshots => Set<TenantSnapshot>();

    /// <summary>MT-01 — async provisioning job tracker.</summary>
    public DbSet<TenantProvisioningJob> TenantProvisioningJobs => Set<TenantProvisioningJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tenant Configuration
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(t => t.TenantId);
            entity.HasIndex(t => t.Slug).IsUnique();
            entity.Property(t => t.Name).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Slug).HasMaxLength(100).IsRequired();
            entity.Property(t => t.AdminEmail).HasMaxLength(255).IsRequired();
            entity.Property(t => t.AdminUsername).HasMaxLength(100).IsRequired();
            entity.Property(t => t.Currency).HasMaxLength(10).HasDefaultValue("XAF");
            entity.Property(t => t.CustomDomain).HasMaxLength(255);
            entity.Property(t => t.UiUrl).HasMaxLength(500);
            entity.Property(t => t.ApiUrl).HasMaxLength(500);
            entity.Property(t => t.LastHealthMessage).HasMaxLength(500);

            // Value Converter for Secrets with AES-256 Encryption
            var secretsConverter = new ValueConverter<TenantSecrets, string>(
                v => SerializeAndEncryptSecrets(v, _encryptionService),
                v => DeserializeAndDecryptSecrets(v, _encryptionService)
            );

            entity.Property(t => t.Secrets)
                .HasConversion(secretsConverter)
                .HasColumnType("longtext");

            // Value Converters for complex JSON structures
            entity.Property(t => t.DomainConfig)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<TenantDomainConfig>(v, JsonOptions) ?? new())
                .HasColumnType("longtext");

            entity.Property(t => t.Branches)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<List<TenantBranchMapping>>(v, JsonOptions) ?? new())
                .HasColumnType("longtext");

            entity.Property(t => t.BackupProviders)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<List<BackupProviderConfig>>(v, JsonOptions) ?? new())
                .HasColumnType("longtext");

            entity.Property(t => t.BackupSchedule)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<BackupScheduleConfig>(v, JsonOptions) ?? new())
                .HasColumnType("longtext");

            entity.Property(t => t.BackupHistory)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<List<TenantBackupJobRecord>>(v, JsonOptions) ?? new())
                .HasColumnType("longtext");

            entity.Property(t => t.AuditTrail)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<List<TenantAuditRecord>>(v, JsonOptions) ?? new())
                .HasColumnType("longtext");

            entity.Property(t => t.ProvisioningLogs)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<List<TenantProvisioningLog>>(v, JsonOptions) ?? new())
                .HasColumnType("longtext");

            // MT-07 — maintenance windows surfaced to the public status page.
            entity.Property(t => t.MaintenanceWindows)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<List<MaintenanceWindow>>(v, JsonOptions) ?? new())
                .HasColumnType("longtext")
                .HasColumnName("maintenance_windows");

            // Wave 18 — payment history surfaced on the Billing page.
            entity.Property(t => t.Payments)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, JsonOptions),
                    v => JsonSerializer.Deserialize<List<TenantPayment>>(v, JsonOptions) ?? new())
                .HasColumnType("longtext")
                .HasColumnName("payments");

            // Wave 18 — subscription lifecycle scalar columns.
            entity.Property(t => t.SubscriptionPlanId).HasMaxLength(64).HasColumnName("subscription_plan_id");
            entity.Property(t => t.SubscriptionStatus).HasConversion<int>().HasColumnName("subscription_status");
            entity.Property(t => t.SubscriptionStartUtc).HasColumnName("subscription_start_utc");
            entity.Property(t => t.SubscriptionEndUtc).HasColumnName("subscription_end_utc");
            entity.Property(t => t.NextBillingAtUtc).HasColumnName("next_billing_at_utc");
            entity.Property(t => t.GracePeriodUntilUtc).HasColumnName("grace_period_until_utc");
            entity.Property(t => t.LastPaymentToken).HasMaxLength(128).HasColumnName("last_payment_token");
        });

        // PortalAccount Configuration
        modelBuilder.Entity<PortalAccount>(entity =>
        {
            entity.ToTable("portal_accounts");
            entity.HasKey(a => a.AccountId);
            entity.HasIndex(a => a.Email).IsUnique();
            entity.Property(a => a.Email).HasMaxLength(255).IsRequired();
            entity.Property(a => a.FullName).HasMaxLength(200).IsRequired();
            entity.Property(a => a.PasswordHash).HasMaxLength(500).IsRequired();
        });

        // SystemRelease Configuration
        modelBuilder.Entity<SystemRelease>(entity =>
        {
            entity.ToTable("system_releases");
            entity.HasKey(r => r.ReleaseId);
            entity.HasIndex(r => r.VersionName).IsUnique();
            entity.Property(r => r.VersionName).HasMaxLength(100).IsRequired();
            entity.Property(r => r.ApiImageTag).HasMaxLength(255).IsRequired();
            entity.Property(r => r.UiImageTag).HasMaxLength(255).IsRequired();
            entity.Property(r => r.DatabaseMigrationTag).HasMaxLength(255);
        });

        // TenantSnapshot Configuration
        modelBuilder.Entity<TenantSnapshot>(entity =>
        {
            entity.ToTable("tenant_snapshots");
            entity.HasKey(s => s.SnapshotId);
            entity.HasIndex(s => s.TenantId);
            entity.Property(s => s.SqlDumpPath).HasMaxLength(1000).IsRequired();
        });

        // MT-01 — TenantProvisioningJob
        modelBuilder.Entity<TenantProvisioningJob>(entity =>
        {
            entity.ToTable("tenant_provisioning_jobs");
            entity.HasKey(j => j.JobId);
            entity.Property(j => j.JobId).HasColumnName("job_id");
            entity.Property(j => j.AccountId).HasColumnName("account_id");
            entity.Property(j => j.StoreName).HasColumnName("store_name").HasMaxLength(200).IsRequired();
            entity.Property(j => j.Slug).HasColumnName("slug").HasMaxLength(80).IsRequired();
            entity.Property(j => j.AdminEmail).HasColumnName("admin_email").HasMaxLength(255).IsRequired();
            entity.Property(j => j.AdminUsername).HasColumnName("admin_username").HasMaxLength(100).IsRequired();
            entity.Property(j => j.AdminPasswordCipher).HasColumnName("admin_password_cipher").HasMaxLength(512);
            entity.Property(j => j.Currency).HasColumnName("currency").HasMaxLength(8);
            entity.Property(j => j.PlanTier).HasColumnName("plan_tier").HasConversion<string>().HasMaxLength(40);
            entity.Property(j => j.CustomDomain).HasColumnName("custom_domain").HasMaxLength(255);
            entity.Property(j => j.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            entity.Property(j => j.StatusDetail).HasColumnName("status_detail").HasMaxLength(1000);
            entity.Property(j => j.FailureReason).HasColumnName("failure_reason").HasMaxLength(2000);
            entity.Property(j => j.TenantId).HasColumnName("tenant_id");
            entity.Property(j => j.DateCreated).HasColumnName("date_created");
            entity.Property(j => j.StartedAt).HasColumnName("started_at");
            entity.Property(j => j.CompletedAt).HasColumnName("completed_at");
            entity.HasIndex(j => j.AccountId);
            // The hosted service polls Status=Pending ORDER BY DateCreated ASC.
            entity.HasIndex(j => new { j.Status, j.DateCreated })
                  .HasDatabaseName("ix_provisioning_jobs_status_date");
        });
    }

    private static string SerializeAndEncryptSecrets(TenantSecrets secrets, ISecretEncryptionService? encryption)
    {
        if (encryption == null)
        {
            return JsonSerializer.Serialize(secrets, JsonOptions);
        }

        // Clone and encrypt secrets
        var encryptedSecrets = new TenantSecrets
        {
            MySqlRootPassword = encryption.Encrypt(secrets.MySqlRootPassword),
            MySqlUserPassword = encryption.Encrypt(secrets.MySqlUserPassword),
            MongoDbRootPassword = encryption.Encrypt(secrets.MongoDbRootPassword),
            JwtSecret = encryption.Encrypt(secrets.JwtSecret),
            MoMoCallbackKey = encryption.Encrypt(secrets.MoMoCallbackKey),
            OtpPepper = string.IsNullOrEmpty(secrets.OtpPepper) ? string.Empty : encryption.Encrypt(secrets.OtpPepper),
            BackupEncryptionKey = string.IsNullOrEmpty(secrets.BackupEncryptionKey) ? string.Empty : encryption.Encrypt(secrets.BackupEncryptionKey),
            SmtpHost = secrets.SmtpHost,
            SmtpPort = secrets.SmtpPort,
            SmtpUsername = secrets.SmtpUsername,
            SmtpPassword = string.IsNullOrEmpty(secrets.SmtpPassword) ? string.Empty : encryption.Encrypt(secrets.SmtpPassword),
            SmtpFromEmail = secrets.SmtpFromEmail,
            SmtpFromName = secrets.SmtpFromName,
            SmtpEnableSsl = secrets.SmtpEnableSsl,
            SmtpIsEnabled = secrets.SmtpIsEnabled,
            SmtpLastTestedAt = secrets.SmtpLastTestedAt,
            SmtpLastTestStatus = secrets.SmtpLastTestStatus
        };

        return JsonSerializer.Serialize(encryptedSecrets, JsonOptions);
    }

    private static TenantSecrets DeserializeAndDecryptSecrets(string json, ISecretEncryptionService? encryption)
    {
        if (string.IsNullOrWhiteSpace(json)) return new TenantSecrets();

        var secrets = JsonSerializer.Deserialize<TenantSecrets>(json, JsonOptions) ?? new TenantSecrets();
        if (encryption == null) return secrets;

        return new TenantSecrets
        {
            MySqlRootPassword = encryption.Decrypt(secrets.MySqlRootPassword),
            MySqlUserPassword = encryption.Decrypt(secrets.MySqlUserPassword),
            MongoDbRootPassword = encryption.Decrypt(secrets.MongoDbRootPassword),
            JwtSecret = encryption.Decrypt(secrets.JwtSecret),
            MoMoCallbackKey = encryption.Decrypt(secrets.MoMoCallbackKey),
            OtpPepper = string.IsNullOrEmpty(secrets.OtpPepper) ? string.Empty : encryption.Decrypt(secrets.OtpPepper),
            BackupEncryptionKey = string.IsNullOrEmpty(secrets.BackupEncryptionKey) ? string.Empty : encryption.Decrypt(secrets.BackupEncryptionKey),
            SmtpHost = secrets.SmtpHost,
            SmtpPort = secrets.SmtpPort,
            SmtpUsername = secrets.SmtpUsername,
            SmtpPassword = string.IsNullOrEmpty(secrets.SmtpPassword) ? string.Empty : encryption.Decrypt(secrets.SmtpPassword),
            SmtpFromEmail = secrets.SmtpFromEmail,
            SmtpFromName = secrets.SmtpFromName,
            SmtpEnableSsl = secrets.SmtpEnableSsl,
            SmtpIsEnabled = secrets.SmtpIsEnabled,
            SmtpLastTestedAt = secrets.SmtpLastTestedAt,
            SmtpLastTestStatus = secrets.SmtpLastTestStatus
        };
    }
}

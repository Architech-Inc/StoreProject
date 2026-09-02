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
            MoMoCallbackKey = encryption.Encrypt(secrets.MoMoCallbackKey)
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
            MoMoCallbackKey = encryption.Decrypt(secrets.MoMoCallbackKey)
        };
    }
}

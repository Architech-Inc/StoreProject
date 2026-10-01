using System.Text;
using System.Text.Json;
using Store.ControlPlane.Models;
using Store.ControlPlane.Services;
using Xunit;

namespace Store.ControlPlane.Tests;

public class TenantBackupArchiverTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly TenantBackupArchiver _archiver;

    public TenantBackupArchiverTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "clexan_test_backups_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
        _archiver = new TenantBackupArchiver();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            try { Directory.Delete(_tempDirectory, recursive: true); } catch { }
        }
    }

    [Fact]
    public async Task CreateEncryptedSnapshot_GeneratesIsolatedFilesAndManifest()
    {
        var tenant = new Tenant
        {
            TenantId = Guid.NewGuid(),
            Slug = "acme-corp",
            BackupSchedule = new BackupScheduleConfig { RetentionCount = 5 }
        };

        var encKey = "TestSuperSecretKey1234567890123456";
        var mySqlData = Encoding.UTF8.GetBytes("CREATE TABLE products (id INT); INSERT INTO products VALUES (1);");
        var mongoData = Encoding.UTF8.GetBytes("{\"collection\": \"audit\", \"data\": [1, 2, 3]}");

        var result = await _archiver.CreateEncryptedSnapshotAsync(
            tenant,
            _tempDirectory,
            encKey,
            rawMySqlDump: mySqlData,
            rawMongoDump: mongoData
        );

        Assert.NotNull(result);
        Assert.Equal(3, result.Files.Count);
        Assert.True(File.Exists(result.ManifestPath));

        var tenantDir = Path.Combine(_tempDirectory, "acme-corp");
        Assert.True(Directory.Exists(tenantDir));

        // Check manifest content
        var manifestJson = await File.ReadAllTextAsync(result.ManifestPath);
        var manifest = JsonSerializer.Deserialize<TenantSnapshotManifest>(manifestJson);
        Assert.NotNull(manifest);
        Assert.Equal("acme-corp", manifest.TenantSlug);
        Assert.True(manifest.IsEncrypted);
        Assert.Equal("AES-256-CBC-PBKDF2", manifest.EncryptionAlgorithm);
        Assert.Equal(5, manifest.RetentionCount);
        Assert.NotEmpty(manifest.MySql.Sha256);
        Assert.NotEmpty(manifest.MongoDb.Sha256);
    }

    [Fact]
    public async Task EncryptionAndDecryption_RoundTripRecoversOriginalPayload()
    {
        var tenant = new Tenant { TenantId = Guid.NewGuid(), Slug = "crypto-tenant" };
        var encKey = "StrongCryptoKey_2026_AES256_PBKDF2!";
        var originalText = "SELECT * FROM sales WHERE tenant_id = 'crypto-tenant'; -- Sensitive financial records";
        var originalBytes = Encoding.UTF8.GetBytes(originalText);

        var result = await _archiver.CreateEncryptedSnapshotAsync(
            tenant,
            _tempDirectory,
            encKey,
            rawMySqlDump: originalBytes
        );

        var mysqlFilename = result.Files.First(f => f.Contains("-mysql-"));
        var mysqlEncPath = Path.Combine(_tempDirectory, tenant.Slug, mysqlFilename);

        // Verify file is encrypted on disk (does not contain original plaintext)
        var encryptedBytes = await File.ReadAllBytesAsync(mysqlEncPath);
        Assert.DoesNotContain("Sensitive financial records", Encoding.Latin1.GetString(encryptedBytes));

        // Decrypt and decompress
        var decryptedBytes = await TenantBackupArchiver.DecryptAndDecompressFileAsync(mysqlEncPath, encKey);
        var decryptedText = Encoding.UTF8.GetString(decryptedBytes);

        Assert.Equal(originalText, decryptedText);
    }

    [Fact]
    public async Task Sha256Checksum_DetectsTampering()
    {
        var tenant = new Tenant { TenantId = Guid.NewGuid(), Slug = "integrity-tenant" };
        var encKey = "IntegrityKey12345678901234567890";
        var payload = Encoding.UTF8.GetBytes("Uncompromised Database Snapshot Content");

        var result = await _archiver.CreateEncryptedSnapshotAsync(
            tenant,
            _tempDirectory,
            encKey,
            rawMySqlDump: payload
        );

        var mysqlFilename = result.Files.First(f => f.Contains("-mysql-"));
        var mysqlEncPath = Path.Combine(_tempDirectory, tenant.Slug, mysqlFilename);
        var expectedSha = result.Checksums[mysqlFilename];

        // Valid file passes check
        var isValid = await TenantBackupArchiver.VerifySha256ChecksumAsync(mysqlEncPath, expectedSha);
        Assert.True(isValid);

        // Tamper with a single byte in the file
        var fileBytes = await File.ReadAllBytesAsync(mysqlEncPath);
        fileBytes[^1] ^= 0xFF;
        await File.WriteAllBytesAsync(mysqlEncPath, fileBytes);

        // Tampered file fails check
        var isTamperedValid = await TenantBackupArchiver.VerifySha256ChecksumAsync(mysqlEncPath, expectedSha);
        Assert.False(isTamperedValid);
    }

    [Fact]
    public async Task RetentionRotation_PrunesOldestSnapshotsAndPreservesIsolatedTenants()
    {
        var tenantA = new Tenant { TenantId = Guid.NewGuid(), Slug = "tenant-alpha" };
        var tenantB = new Tenant { TenantId = Guid.NewGuid(), Slug = "tenant-beta" };
        var key = "MultiTenantKey123456789012345678";

        // Create 1 snapshot for Tenant B
        await _archiver.CreateEncryptedSnapshotAsync(tenantB, _tempDirectory, key, retentionCountOverride: 10);
        var tenantBDir = Path.Combine(_tempDirectory, "tenant-beta");
        var tenantBInitialFiles = Directory.GetFiles(tenantBDir).Length;
        Assert.True(tenantBInitialFiles >= 3);

        // Create 5 snapshots for Tenant A with retention count of 2
        for (int i = 0; i < 5; i++)
        {
            await _archiver.CreateEncryptedSnapshotAsync(
                tenantA,
                _tempDirectory,
                key,
                retentionCountOverride: 2
            );
            await Task.Delay(15); // ensure distinct timestamp
        }

        var tenantADir = Path.Combine(_tempDirectory, "tenant-alpha");
        var tenantAManifests = Directory.GetFiles(tenantADir, "tenant-alpha-manifest-*.json");

        // Tenant A must only keep 2 manifests (and corresponding snapshot files)
        Assert.Equal(2, tenantAManifests.Length);

        // Tenant B's snapshots must remain completely untouched (isolated retention)
        var tenantBAfterFiles = Directory.GetFiles(tenantBDir).Length;
        Assert.Equal(tenantBInitialFiles, tenantBAfterFiles);
    }
}

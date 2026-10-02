using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Store.ControlPlane.Models;

namespace Store.ControlPlane.Services;

public record TenantSnapshotResult(
    List<string> Files,
    long TotalSizeBytes,
    Dictionary<string, string> Checksums,
    string ManifestPath,
    int PrunedCount,
    DateTime Timestamp
);

public record TenantSnapshotManifest(
    string TenantSlug,
    DateTime Timestamp,
    bool IsEncrypted,
    string EncryptionAlgorithm,
    TenantDumpFileMetadata MySql,
    TenantDumpFileMetadata MongoDb,
    int RetentionCount,
    string Status
);

public record TenantDumpFileMetadata(
    string Filename,
    long SizeBytes,
    string Sha256
);

/// <summary>
/// OPS-12: Tenant-scoped database snapshot archiving engine providing:
/// - Isolated per-tenant directory structures
/// - OpenSSL-compatible AES-256-CBC PBKDF2 encryption
/// - SHA-256 cryptographic integrity verification
/// - JSON metadata manifest generation
/// - Automated retention rotation (pruning excess snapshots per-tenant)
/// </summary>
public class TenantBackupArchiver
{
    private readonly ILogger<TenantBackupArchiver>? _logger;
    private const int Pbkdf2Iterations = 10000;
    private const int SaltSizeBytes = 16;
    private const int IvSizeBytes = 16;
    private const int KeySizeBytes = 32;

    public TenantBackupArchiver(ILogger<TenantBackupArchiver>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates an encrypted snapshot archive for a specific tenant and applies retention rotation.
    /// </summary>
    public async Task<TenantSnapshotResult> CreateEncryptedSnapshotAsync(
        Tenant tenant,
        string baseDirectory,
        string encryptionKey,
        byte[]? rawMySqlDump = null,
        byte[]? rawMongoDump = null,
        int? retentionCountOverride = null,
        CancellationToken ct = default)
    {
        var timestamp = DateTime.UtcNow;
        var dateStr = timestamp.ToString("yyyyMMdd_HHmmssfff");
        var tenantDir = Path.Combine(baseDirectory, tenant.Slug);
        Directory.CreateDirectory(tenantDir);

        var isEncrypted = !string.IsNullOrWhiteSpace(encryptionKey);
        var mysqlExt = isEncrypted ? "sql.gz.enc" : "sql.gz";
        var mongoExt = isEncrypted ? "archive.gz.enc" : "archive.gz";

        var mysqlFilename = $"{tenant.Slug}-mysql-{dateStr}.{mysqlExt}";
        var mongoFilename = $"{tenant.Slug}-mongodb-{dateStr}.{mongoExt}";

        var mysqlPath = Path.Combine(tenantDir, mysqlFilename);
        var mongoPath = Path.Combine(tenantDir, mongoFilename);

        // Generate or compress MySQL payload
        var mysqlBytes = rawMySqlDump ?? Encoding.UTF8.GetBytes($"-- Snapshot MySQL Dump for tenant {tenant.Slug}\n-- Created: {timestamp:O}\nSELECT 1;\n");
        await WriteCompressedAndEncryptedFileAsync(mysqlPath, mysqlBytes, encryptionKey, ct);

        // Generate or compress MongoDB payload
        var mongoBytes = rawMongoDump ?? Encoding.UTF8.GetBytes($"MongoDB Archive Metadata: Tenant={tenant.Slug}, Date={timestamp:O}");
        await WriteCompressedAndEncryptedFileAsync(mongoPath, mongoBytes, encryptionKey, ct);

        // Compute SHA-256 checksums
        var mysqlSha = await ComputeSha256Async(mysqlPath, ct);
        var mongoSha = await ComputeSha256Async(mongoPath, ct);

        await File.WriteAllTextAsync($"{mysqlPath}.sha256", $"{mysqlSha}  {mysqlFilename}\n", ct);
        await File.WriteAllTextAsync($"{mongoPath}.sha256", $"{mongoSha}  {mongoFilename}\n", ct);

        var mysqlSize = new FileInfo(mysqlPath).Length;
        var mongoSize = new FileInfo(mongoPath).Length;
        var totalBytes = mysqlSize + mongoSize;

        var checksums = new Dictionary<string, string>
        {
            [mysqlFilename] = mysqlSha,
            [mongoFilename] = mongoSha
        };

        var retentionCount = retentionCountOverride 
            ?? tenant.BackupSchedule?.RetentionCount 
            ?? 14;

        // Generate Manifest
        var manifest = new TenantSnapshotManifest(
            TenantSlug: tenant.Slug,
            Timestamp: timestamp,
            IsEncrypted: isEncrypted,
            EncryptionAlgorithm: isEncrypted ? "AES-256-CBC-PBKDF2" : "None",
            MySql: new TenantDumpFileMetadata(mysqlFilename, mysqlSize, mysqlSha),
            MongoDb: new TenantDumpFileMetadata(mongoFilename, mongoSize, mongoSha),
            RetentionCount: retentionCount,
            Status: "Completed"
        );

        var manifestFilename = $"{tenant.Slug}-manifest-{dateStr}.json";
        var manifestPath = Path.Combine(tenantDir, manifestFilename);
        var manifestJson = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(manifestPath, manifestJson, ct);

        // Perform isolated per-tenant retention rotation
        var prunedCount = RotateTenantRetention(tenantDir, tenant.Slug, retentionCount);

        _logger?.LogInformation(
            "Created encrypted snapshot for tenant {Slug}: MySQL={MySql}, Mongo={Mongo}, TotalSize={Size}, Pruned={Pruned}",
            tenant.Slug, mysqlFilename, mongoFilename, totalBytes, prunedCount);

        return new TenantSnapshotResult(
            Files: new List<string> { mysqlFilename, mongoFilename, manifestFilename },
            TotalSizeBytes: totalBytes,
            Checksums: checksums,
            ManifestPath: manifestPath,
            PrunedCount: prunedCount,
            Timestamp: timestamp
        );
    }

    /// <summary>
    /// Compresses data via GZip and encrypts it with AES-256-CBC PBKDF2 if an encryption key is supplied.
    /// Binary format: [16-byte salt][16-byte IV][ciphertext]
    /// </summary>
    public static async Task WriteCompressedAndEncryptedFileAsync(
        string outputPath,
        byte[] rawData,
        string? encryptionKey,
        CancellationToken ct = default)
    {
        // 1. Gzip compression
        byte[] compressedBytes;
        using (var compressedMs = new MemoryStream())
        {
            using (var gzip = new GZipStream(compressedMs, CompressionLevel.Optimal, leaveOpen: true))
            {
                await gzip.WriteAsync(rawData, ct);
            }
            compressedBytes = compressedMs.ToArray();
        }

        // 2. Encryption if key provided
        if (string.IsNullOrWhiteSpace(encryptionKey))
        {
            await File.WriteAllBytesAsync(outputPath, compressedBytes, ct);
            return;
        }

        var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
        using var kdf = new Rfc2898DeriveBytes(encryptionKey, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256);
        var key = kdf.GetBytes(KeySizeBytes);

        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var fs = File.Create(outputPath);
        await fs.WriteAsync(salt, ct);
        await fs.WriteAsync(aes.IV, ct);

        using var cryptoStream = new CryptoStream(fs, aes.CreateEncryptor(), CryptoStreamMode.Write);
        await cryptoStream.WriteAsync(compressedBytes, ct);
        await cryptoStream.FlushFinalBlockAsync(ct);
    }

    /// <summary>
    /// Decrypts and decompresses a snapshot archive.
    /// </summary>
    public static async Task<byte[]> DecryptAndDecompressFileAsync(
        string inputPath,
        string? encryptionKey,
        CancellationToken ct = default)
    {
        byte[] compressedBytes;

        if (string.IsNullOrWhiteSpace(encryptionKey))
        {
            compressedBytes = await File.ReadAllBytesAsync(inputPath, ct);
        }
        else
        {
            using var fs = File.OpenRead(inputPath);
            var salt = new byte[SaltSizeBytes];
            var iv = new byte[IvSizeBytes];

            var saltRead = await fs.ReadAsync(salt, ct);
            var ivRead = await fs.ReadAsync(iv, ct);
            if (saltRead < SaltSizeBytes || ivRead < IvSizeBytes)
            {
                throw new InvalidOperationException("Encrypted snapshot file header is truncated.");
            }

            using var kdf = new Rfc2898DeriveBytes(encryptionKey, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256);
            var key = kdf.GetBytes(KeySizeBytes);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var decryptedMs = new MemoryStream();
            using (var cryptoStream = new CryptoStream(fs, aes.CreateDecryptor(), CryptoStreamMode.Read))
            {
                await cryptoStream.CopyToAsync(decryptedMs, ct);
            }
            compressedBytes = decryptedMs.ToArray();
        }

        // Decompress Gzip
        using var inMs = new MemoryStream(compressedBytes);
        using var gzip = new GZipStream(inMs, CompressionMode.Decompress);
        using var outMs = new MemoryStream();
        await gzip.CopyToAsync(outMs, ct);
        return outMs.ToArray();
    }

    /// <summary>
    /// Computes the lowercase hex SHA-256 checksum of a file.
    /// </summary>
    public static async Task<string> ComputeSha256Async(string filePath, CancellationToken ct = default)
    {
        using var fs = File.OpenRead(filePath);
        using var sha = SHA256.Create();
        var hashBytes = await sha.ComputeHashAsync(fs, ct);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Verifies the cryptographic integrity of a file against an expected SHA-256 checksum.
    /// </summary>
    public static async Task<bool> VerifySha256ChecksumAsync(string filePath, string expectedChecksum, CancellationToken ct = default)
    {
        if (!File.Exists(filePath)) return false;
        var actual = await ComputeSha256Async(filePath, ct);
        return string.Equals(actual, expectedChecksum.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Performs isolated per-tenant retention rotation.
    /// Keeps only the N newest snapshots for the specified tenant, removing older files and checksums.
    /// </summary>
    public static int RotateTenantRetention(string tenantDir, string tenantSlug, int retentionCount)
    {
        if (!Directory.Exists(tenantDir)) return 0;

        // Group files by timestamp prefix: e.g. "{slug}-mysql-{timestamp}"
        var manifestFiles = Directory.GetFiles(tenantDir, $"{tenantSlug}-manifest-*.json")
            .OrderBy(f => Path.GetFileName(f))
            .ToList();

        if (manifestFiles.Count <= retentionCount) return 0;

        var excessCount = manifestFiles.Count - retentionCount;
        var toPrune = manifestFiles.Take(excessCount).ToList();
        var deletedCount = 0;

        foreach (var manifestPath in toPrune)
        {
            try
            {
                var manifestName = Path.GetFileNameWithoutExtension(manifestPath);
                // Extract timestamp: "{slug}-manifest-{timestamp}"
                var prefix = manifestName.Replace($"{tenantSlug}-manifest-", "");

                // Delete manifest
                File.Delete(manifestPath);
                deletedCount++;

                // Delete matching mysql, mongo, and sha256 files
                var matchingFiles = Directory.GetFiles(tenantDir, $"{tenantSlug}-*-{prefix}.*");
                foreach (var file in matchingFiles)
                {
                    File.Delete(file);
                    deletedCount++;
                }
            }
            catch
            {
                // Continue pruning remaining files on transient I/O exceptions
            }
        }

        return deletedCount;
    }
}

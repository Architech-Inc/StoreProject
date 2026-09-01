# 04 — Store.TenantPortal Backup & Cloud Storage Specification

**Status:** Draft
**Version:** 1.0
**Date:** September 2026

---

## 1. Overview

Tenants can configure up to three external backup providers:

| Provider | Auth Method | SDK |
|:---|:---|:---|
| **OneDrive** | OAuth2 Authorization Code (Microsoft Graph) | `Microsoft.Graph` v5 |
| **Google Drive** | OAuth2 Authorization Code (Google APIs) | `Google.Apis.Drive.v3` |
| **S3 / MinIO** | Static credentials (Access Key + Secret) | `AWSSDK.S3` |

Backups consist of two files per run:
- MySQL dump: `{slug}-mysql-{yyyy-MM-dd-HHmm}.sql.gz`
- MongoDB dump: `{slug}-mongodb-{yyyy-MM-dd-HHmm}.archive.gz`

---

## 2. OneDrive Integration (Microsoft Graph)

### 2.1 OAuth2 Authorization Code Flow

```
Portal                       Microsoft Identity Platform          OneDrive
  |                                    |                             |
  |-- [Connect OneDrive] clicked       |                             |
  |                                    |                             |
  |-- Build auth URL:                  |                             |
  |   https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize
  |   ?client_id={clientId}            |                             |
  |   &response_type=code              |                             |
  |   &redirect_uri={callbackUrl}      |                             |
  |   &scope=Files.ReadWrite.AppFolder offline_access
  |   &state={encrypted-tenant-id}     |                             |
  |                                    |                             |
  |-- Open popup window to auth URL -> |                             |
  |                                    |                             |
  |             User consents          |                             |
  |                                    |-- Redirect to callback URL  |
  |                                    |   with ?code={authCode}     |
  |                                    |   &state={tenant-id}        |
  |                                    |                             |
  |-- /oauth/microsoft/callback        |                             |
  |   Verify state, extract tenantId   |                             |
  |   POST token exchange:             |                             |
  |   https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token
  |   grant_type=authorization_code    |                             |
  |   code={authCode}                  |                             |
  |                                    |-- Returns access_token,     |
  |                                    |   refresh_token, expires_in |
  |                                    |                             |
  |-- Encrypt refresh_token (AES-256)  |                             |
  |-- Store in BackupProvider record   |                             |
  |-- Close popup, refresh parent page |                             |
```

### 2.2 Scopes Required

```
Files.ReadWrite.AppFolder    - Read/write access to app-specific folder only
                               (/Apps/ClexAnFoods/{tenantSlug}/backups/)
offline_access               - Required to receive a refresh_token
```

The `AppFolder` scope limits OneDrive access to a dedicated app folder only — the app
cannot read or modify the tenant's personal OneDrive files. This is a significant security
and privacy advantage over requesting `Files.ReadWrite`.

### 2.3 Token Refresh Strategy

Access tokens expire in ~1 hour. Before each backup job:

```csharp
public async Task<string> GetValidAccessTokenAsync(Guid tenantId, CancellationToken ct)
{
    var provider = await GetProviderAsync(tenantId, BackupProviderType.OneDrive, ct);

    if (provider.OAuthTokenExpiry > DateTime.UtcNow.AddMinutes(5))
    {
        return AesDecrypt(provider.OAuthAccessToken!);   // Still valid
    }

    // Refresh using stored refresh token
    var refreshToken = AesDecrypt(provider.OAuthRefreshToken!);
    var newTokens = await _graphClient.RefreshTokenAsync(refreshToken, ct);

    provider.OAuthAccessToken = AesEncrypt(newTokens.AccessToken);
    provider.OAuthRefreshToken = AesEncrypt(newTokens.RefreshToken);
    provider.OAuthTokenExpiry = DateTime.UtcNow.AddSeconds(newTokens.ExpiresIn - 60);
    await _tenantRepo.SaveAsync(/* ... */);

    return newTokens.AccessToken;
}
```

### 2.4 Upload Implementation

```csharp
// Upload MySQL dump to OneDrive App Folder
var accessToken = await GetValidAccessTokenAsync(tenantId, ct);
var graphClient = new GraphServiceClient(
    new BaseBearerTokenAuthenticationProvider(new StaticTokenProvider(accessToken)));

var uploadPath = $"/Apps/ClexAnFoods/{slug}/backups/{filename}";
using var stream = File.OpenRead(dumpFilePath);

await graphClient.Me.Drive.Special["approot"]
    .ItemWithPath(uploadPath)
    .Content
    .PutAsync(stream, cancellationToken: ct);
```

---

## 3. Google Drive Integration

### 3.1 OAuth2 Authorization Code Flow

Same pattern as OneDrive but using Google's OAuth2 endpoints:

```
Auth URL:     https://accounts.google.com/o/oauth2/v2/auth
Token URL:    https://oauth2.googleapis.com/token
Scope:        https://www.googleapis.com/auth/drive.file
Redirect URI: https://portal.store.{rootDomain}/oauth/google/callback
```

The `drive.file` scope grants access **only to files created by the app** — the app cannot
read existing files in the user's Google Drive. New backup files are uploaded into a folder
the app creates: `ClexAn Backups / {tenantSlug}/`.

### 3.2 Upload Implementation

```csharp
using Google.Apis.Drive.v3;
using Google.Apis.Auth.OAuth2;

// Use stored refresh token to get a UserCredential
var credential = new UserCredential(
    new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
    {
        ClientSecrets = new ClientSecrets
        {
            ClientId = _config["OAuth:Google:ClientId"],
            ClientSecret = _config["OAuth:Google:ClientSecret"]
        },
        Scopes = new[] { DriveService.Scope.DriveFile }
    }),
    "user",
    new TokenResponse
    {
        RefreshToken = AesDecrypt(provider.OAuthRefreshToken!)
    });

var driveService = new DriveService(new Google.Apis.Services.BaseClientService.Initializer
{
    HttpClientInitializer = credential,
    ApplicationName = "ClexAn Foods Store Portal"
});

// Ensure folder exists
var folderId = await EnsureFolderAsync(driveService, slug, ct);

// Upload file
var fileMetadata = new Google.Apis.Drive.v3.Data.File
{
    Name = filename,
    Parents = new[] { folderId }
};

using var stream = File.OpenRead(dumpFilePath);
var request = driveService.Files.Create(fileMetadata, stream, "application/gzip");
await request.UploadAsync(ct);
```

---

## 4. S3 / MinIO Integration

### 4.1 Credential Model

Unlike OneDrive and Google Drive, S3/MinIO uses static credentials — no OAuth flow needed.
Credentials are stored encrypted in the `BackupProvider` record.

```csharp
// Additional fields for S3 providers only (stored in a JSON extension field)
public class S3BackupConfig
{
    public string Endpoint { get; set; } = string.Empty;    // "" for AWS S3, URL for MinIO
    public string Region { get; set; } = "us-east-1";
    public string BucketName { get; set; } = string.Empty;
    public string AccessKeyId { get; set; } = string.Empty;         // AES encrypted
    public string SecretAccessKey { get; set; } = string.Empty;     // AES encrypted
    public string? KeyPrefix { get; set; }                          // Optional folder prefix
}
```

### 4.2 Upload Implementation

```csharp
using Amazon.S3;
using Amazon.S3.Transfer;

var s3Config = new AmazonS3Config
{
    ServiceURL = string.IsNullOrEmpty(s3Cfg.Endpoint) ? null : s3Cfg.Endpoint,
    ForcePathStyle = !string.IsNullOrEmpty(s3Cfg.Endpoint),   // MinIO requires path-style
    RegionEndpoint = RegionEndpoint.GetBySystemName(s3Cfg.Region)
};

var s3Client = new AmazonS3Client(
    AesDecrypt(s3Cfg.AccessKeyId),
    AesDecrypt(s3Cfg.SecretAccessKey),
    s3Config);

var prefix = string.IsNullOrEmpty(s3Cfg.KeyPrefix) ? slug : $"{s3Cfg.KeyPrefix}/{slug}";
var key = $"{prefix}/backups/{filename}";

var transferUtility = new TransferUtility(s3Client);
await transferUtility.UploadAsync(dumpFilePath, s3Cfg.BucketName, key, ct);
```

---

## 5. Backup Job Architecture

### 5.1 BackupJobWorker (BackgroundService in Store.ControlPlane)

A new `BackupJobWorker` runs in `Store.ControlPlane` alongside `TenantHealthMonitorWorker`.
It evaluates tenant backup schedules every minute (cron tick) and triggers jobs as needed.

```csharp
public class BackupJobWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
            var backupSvc = scope.ServiceProvider.GetRequiredService<IBackupOrchestrationService>();

            var tenants = await repo.GetAllAsync(stoppingToken);
            var now = DateTime.UtcNow;

            foreach (var tenant in tenants.Where(t => t.Status == TenantStatus.Active))
            {
                if (ShouldRunBackup(tenant, now))
                {
                    await backupSvc.RunBackupAsync(tenant.TenantId, stoppingToken);
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private static bool ShouldRunBackup(Tenant tenant, DateTime now)
    {
        // Parse tenant.BackupScheduleCron and evaluate against now using NCrontab
        // Return true if the cron expression matches the current minute
    }
}
```

### 5.2 Backup Orchestration Service

```csharp
public interface IBackupOrchestrationService
{
    Task RunBackupAsync(Guid tenantId, CancellationToken ct = default);
}

// Implementation:
// 1. docker exec {slug}-mysql mysqldump ... | gzip > /tmp/{filename}.sql.gz
// 2. docker exec {slug}-mongodb mongodump --archive --gzip > /tmp/{filename}.archive.gz
// 3. For each enabled BackupProvider: upload both files
// 4. Apply retention: delete files older than BackupRetentionDays
// 5. Record BackupJob entry (success/failure, timestamps, sizes)
// 6. Update provider.LastBackupAt and LastBackupSuccess
// 7. Persist tenant record
```

### 5.3 Backup Job History Record

```csharp
public class BackupJobRecord
{
    public Guid JobId { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string DatabaseType { get; set; } = string.Empty;   // "MySQL" or "MongoDB"
    public long FileSizeBytes { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsManual { get; set; }   // true if triggered by [Backup Now]
}
```

Stored in `App_Data/backup-jobs.json` — a separate JSON file from `tenants.json` to avoid
locking conflicts between the health monitor and backup worker.

---

## 6. Token Encryption Design

All OAuth refresh tokens, access tokens, and S3 secret keys are stored encrypted at rest.

### 6.1 Algorithm: AES-256-GCM

```csharp
public static class AesGcmEncryption
{
    // Key is derived from environment variable "ControlPlane:EncryptionKey" (32 bytes, base64)
    // Each encryption produces a fresh 12-byte nonce, stored prepended to the ciphertext

    public static string Encrypt(string plaintext, byte[] key)
    {
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];   // 12 bytes
        RandomNumberGenerator.Fill(nonce);

        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];       // 16 bytes

        using var aes = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        // Format: base64(nonce + tag + ciphertext)
        var result = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        nonce.CopyTo(result, 0);
        tag.CopyTo(result, nonce.Length);
        cipherBytes.CopyTo(result, nonce.Length + tag.Length);

        return Convert.ToBase64String(result);
    }

    public static string Decrypt(string ciphertext, byte[] key)
    {
        var data = Convert.FromBase64String(ciphertext);
        var nonce = data[..12];
        var tag = data[12..28];
        var cipher = data[28..];

        var plainBytes = new byte[cipher.Length];
        using var aes = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
        aes.Decrypt(nonce, cipher, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
```

### 6.2 Key Management

- Encryption key: 256-bit (32 bytes), stored as base64 in environment variable `ControlPlane__EncryptionKey`
- Key is never written to `tenants.json` or any file
- Key rotation: re-encrypt all stored tokens with new key on rotation (manual procedure documented in runbook)

---

## 7. Retention Policy Implementation

After each successful upload, apply retention to the remote storage:

```
OneDrive:     List files in /Apps/ClexAnFoods/{slug}/backups/
              Delete files where file.createdDateTime < (now - RetentionDays)

Google Drive: List files in folder {folderId} where name contains slug
              Delete files where createdTime < (now - RetentionDays)

S3/MinIO:     ListObjectsV2 with prefix "{prefix}/backups/"
              DeleteObjects where LastModified < (now - RetentionDays)
```

---

## 8. Manual Backup Trigger

`POST /api/control/tenants/{id}/backups/trigger`

- Enqueues a backup job marked `IsManual = true`
- Job runs asynchronously; endpoint returns `202 Accepted`
- Portal polls `GET /api/control/tenants/{id}/backups/history` every 5 seconds until a new job with `StartedAt > requestTime` appears
- On completion, Portal shows toast notification with result

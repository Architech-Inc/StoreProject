<#
.SYNOPSIS
    OPS-12: Instantly creates a tenant-scoped or standalone MySQL and MongoDB snapshot
    from running containers with optional AES-256-CBC PBKDF2 encryption, SHA-256
    integrity checksums, and retention rotation.
.EXAMPLE
    .\scripts\backup-now.ps1 -TenantSlug "kim-sm" -EncryptionKey "MySecretKey123!"
#>
param (
    [Parameter(Mandatory=$false)]
    [string]$TenantSlug = "",

    [Parameter(Mandatory=$false)]
    [string]$OutputDir = "./backups",

    [Parameter(Mandatory=$false)]
    [string]$MySqlContainer = "",

    [Parameter(Mandatory=$false)]
    [string]$MongoContainer = "",

    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = "",

    [Parameter(Mandatory=$false)]
    [string]$EncryptionKey = $env:BACKUP_ENCRYPTION_KEY,

    [Parameter(Mandatory=$false)]
    [int]$RetentionCount = 14
)

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

if ($TenantSlug) {
    if (-not $MySqlContainer) { $MySqlContainer = "${TenantSlug}-mysql" }
    if (-not $MongoContainer) { $MongoContainer = "${TenantSlug}-mongodb" }
    if (-not $DatabaseName) { $DatabaseName = "store_${TenantSlug}" }
    $targetDir = Join-Path $OutputDir $TenantSlug
} else {
    if (-not $MySqlContainer) { $MySqlContainer = "store-mysql" }
    if (-not $MongoContainer) { $MongoContainer = "store-mongodb" }
    if (-not $DatabaseName) { $DatabaseName = "store_db_v2" }
    $targetDir = Join-Path $OutputDir "standalone"
}

if (-not (Test-Path $targetDir)) {
    New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
}

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "Creating instant backup snapshot ($timestamp)" -ForegroundColor Cyan
Write-Host "Tenant:       $(if ($TenantSlug) { $TenantSlug } else { 'Standalone' })" -ForegroundColor Cyan
Write-Host "MySQL:        $MySqlContainer (DB: $DatabaseName)" -ForegroundColor Cyan
Write-Host "MongoDB:      $MongoContainer" -ForegroundColor Cyan
Write-Host "Target Dir:   $targetDir" -ForegroundColor Cyan
Write-Host "Encryption:   $(if ($EncryptionKey) { 'AES-256-CBC Enabled' } else { 'Disabled' })" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

function Encrypt-FilePBKDF2 {
    param(
        [string]$InputPath,
        [string]$OutputPath,
        [string]$Password
    )
    $salt = New-Object byte[] 16
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($salt)

    $deriveBytes = New-Object System.Security.Cryptography.Rfc2898DeriveBytes($Password, $salt, 10000, [System.Security.Cryptography.HashAlgorithmName]::SHA256)
    $key = $deriveBytes.GetBytes(32)

    $aes = [System.Security.Cryptography.Aes]::Create()
    $aes.Key = $key
    $aes.GenerateIV()

    $inStream = [System.IO.File]::OpenRead($InputPath)
    $outStream = [System.IO.File]::Create($OutputPath)

    # Magic: "Salted__" + 8-byte salt (OpenSSL compatible) or [16 salt][16 IV][cipher]
    $outStream.Write($salt, 0, $salt.Length)
    $outStream.Write($aes.IV, 0, $aes.IV.Length)

    $cryptoStream = New-Object System.Security.Cryptography.CryptoStream($outStream, $aes.CreateEncryptor(), [System.Security.Cryptography.CryptoStreamMode]::Write)
    $inStream.CopyTo($cryptoStream)

    $cryptoStream.FlushFinalBlock()
    $cryptoStream.Dispose()
    $inStream.Dispose()
    $outStream.Dispose()
    $aes.Dispose()
}

# 1. MySQL Dump
$tempSqlFile = Join-Path $targetDir "${DatabaseName}_${timestamp}.sql"
Write-Host "Backing up MySQL database '$DatabaseName' from container '$MySqlContainer'..." -ForegroundColor Yellow

docker exec $MySqlContainer mysqldump -u root -prootpassword --single-transaction --quick $DatabaseName > $tempSqlFile

if ($LASTEXITCODE -eq 0 -and (Test-Path $tempSqlFile)) {
    if ($EncryptionKey) {
        $finalSqlFile = Join-Path $targetDir "${DatabaseName}_${timestamp}.sql.gz.enc"
        Encrypt-FilePBKDF2 -InputPath $tempSqlFile -OutputPath $finalSqlFile -Password $EncryptionKey
        Remove-Item $tempSqlFile -Force
    } else {
        $finalSqlFile = $tempSqlFile
    }
    
    $hash = (Get-FileHash -Path $finalSqlFile -Algorithm SHA256).Hash.ToLower()
    Set-Content -Path "${finalSqlFile}.sha256" -Value "$hash  $(Split-Path $finalSqlFile -Leaf)"
    Write-Host "MySQL snapshot created: $finalSqlFile" -ForegroundColor Green
} else {
    Write-Host "MySQL backup failed!" -ForegroundColor Red
}

# 2. MongoDB Dump
$tempMongoArchive = Join-Path $targetDir "mongodb_${timestamp}.archive.gz"
Write-Host "Backing up MongoDB from container '$MongoContainer'..." -ForegroundColor Yellow

docker exec $MongoContainer mongodump -u admin -padminpassword --authenticationDatabase=admin --gzip --archive > $tempMongoArchive

if ($LASTEXITCODE -eq 0 -and (Test-Path $tempMongoArchive)) {
    if ($EncryptionKey) {
        $finalMongoFile = Join-Path $targetDir "mongodb_${timestamp}.archive.gz.enc"
        Encrypt-FilePBKDF2 -InputPath $tempMongoArchive -OutputPath $finalMongoFile -Password $EncryptionKey
        Remove-Item $tempMongoArchive -Force
    } else {
        $finalMongoFile = $tempMongoArchive
    }

    $hash = (Get-FileHash -Path $finalMongoFile -Algorithm SHA256).Hash.ToLower()
    Set-Content -Path "${finalMongoFile}.sha256" -Value "$hash  $(Split-Path $finalMongoFile -Leaf)"
    Write-Host "MongoDB snapshot created: $finalMongoFile" -ForegroundColor Green
} else {
    Write-Host "MongoDB backup failed!" -ForegroundColor Red
}

# 3. Retention Rotation
$sqlFiles = Get-ChildItem -Path $targetDir -File | Where-Object { $_.Name -match "\.sql(\.gz)?(\.enc)?$" } | Sort-Object CreationTime
if ($sqlFiles.Count -gt $RetentionCount) {
    $excess = $sqlFiles.Count - $RetentionCount
    $sqlFiles | Select-Object -First $excess | ForEach-Object {
        Write-Host "Pruning excess MySQL backup: $($_.Name)" -ForegroundColor DarkGray
        Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue
        Remove-Item "$($_.FullName).sha256" -Force -ErrorAction SilentlyContinue
    }
}

$mongoFiles = Get-ChildItem -Path $targetDir -File | Where-Object { $_.Name -match "mongodb_.*\.archive(\.gz)?(\.enc)?$" } | Sort-Object CreationTime
if ($mongoFiles.Count -gt $RetentionCount) {
    $excess = $mongoFiles.Count - $RetentionCount
    $mongoFiles | Select-Object -First $excess | ForEach-Object {
        Write-Host "Pruning excess MongoDB backup: $($_.Name)" -ForegroundColor DarkGray
        Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue
        Remove-Item "$($_.FullName).sha256" -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "Instant backup complete! All snapshots saved in $targetDir" -ForegroundColor Green

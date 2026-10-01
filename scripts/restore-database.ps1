<#
.SYNOPSIS
    OPS-12: Restores MySQL and MongoDB database snapshots from plaintext, gzip,
    or AES-256-CBC PBKDF2 encrypted archives into container instances.
.EXAMPLE
    .\scripts\restore-database.ps1 -MySqlBackup "./backups/kim-sm/store_kim-sm_20261001.sql.gz.enc" -MySqlContainer "kim-sm-mysql" -DatabaseName "store_kim-sm" -EncryptionKey "MySecret123"
#>
param (
    [Parameter(Mandatory=$false)]
    [string]$MySqlBackup = "",

    [Parameter(Mandatory=$false)]
    [string]$MongoBackup = "",

    [Parameter(Mandatory=$false)]
    [string]$MySqlContainer = "store-mysql",

    [Parameter(Mandatory=$false)]
    [string]$MongoContainer = "store-mongodb",

    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = "store_db_v2",

    [Parameter(Mandatory=$false)]
    [string]$EncryptionKey = $env:BACKUP_ENCRYPTION_KEY
)

function Verify-Checksum {
    param([string]$FilePath)
    $shaFile = "${FilePath}.sha256"
    if (Test-Path $shaFile) {
        Write-Host "Verifying SHA-256 checksum for $(Split-Path $FilePath -Leaf)..." -ForegroundColor Yellow
        $expectedHash = (Get-Content $shaFile | Select-Object -First 1).Split(" ")[0].Trim().ToLower()
        $actualHash = (Get-FileHash -Path $FilePath -Algorithm SHA256).Hash.ToLower()
        if ($expectedHash -ne $actualHash) {
            Write-Error "SHA-256 verification failed! Expected: $expectedHash, Got: $actualHash"
            exit 1
        }
        Write-Host "Checksum verified successfully." -ForegroundColor Green
    }
}

function Decrypt-FilePBKDF2 {
    param(
        [string]$InputPath,
        [string]$OutputPath,
        [string]$Password
    )
    $inStream = [System.IO.File]::OpenRead($InputPath)
    $salt = New-Object byte[] 16
    $iv = New-Object byte[] 16

    $inStream.Read($salt, 0, 16) | Out-Null
    $inStream.Read($iv, 0, 16) | Out-Null

    $deriveBytes = New-Object System.Security.Cryptography.Rfc2898DeriveBytes($Password, $salt, 10000, [System.Security.Cryptography.HashAlgorithmName]::SHA256)
    $key = $deriveBytes.GetBytes(32)

    $aes = [System.Security.Cryptography.Aes]::Create()
    $aes.Key = $key
    $aes.IV = $iv

    $outStream = [System.IO.File]::Create($OutputPath)
    $cryptoStream = New-Object System.Security.Cryptography.CryptoStream($inStream, $aes.CreateDecryptor(), [System.Security.Cryptography.CryptoStreamMode]::Read)
    $cryptoStream.CopyTo($outStream)

    $cryptoStream.Dispose()
    $outStream.Dispose()
    $inStream.Dispose()
    $aes.Dispose()
}

# 1. Restore MySQL
if ($MySqlBackup -and (Test-Path $MySqlBackup)) {
    Verify-Checksum $MySqlBackup
    Write-Host "Restoring MySQL database '$DatabaseName' from '$MySqlBackup' into '$MySqlContainer'..." -ForegroundColor Yellow

    if ($MySqlBackup.EndsWith(".enc")) {
        if (-not $EncryptionKey) {
            Write-Error "Backup file is encrypted, but -EncryptionKey parameter or BACKUP_ENCRYPTION_KEY environment variable is missing."
            exit 1
        }
        $tempDecrypted = [System.IO.Path]::GetTempFileName()
        try {
            Write-Host "Decrypting snapshot..." -ForegroundColor Yellow
            Decrypt-FilePBKDF2 -InputPath $MySqlBackup -OutputPath $tempDecrypted -Password $EncryptionKey
            Get-Content $tempDecrypted | docker exec -i $MySqlContainer mysql -u root -prootpassword $DatabaseName
            Write-Host "MySQL restore complete." -ForegroundColor Green
        } finally {
            if (Test-Path $tempDecrypted) { Remove-Item $tempDecrypted -Force }
        }
    } else {
        Get-Content $MySqlBackup | docker exec -i $MySqlContainer mysql -u root -prootpassword $DatabaseName
        Write-Host "MySQL restore complete." -ForegroundColor Green
    }
}

# 2. Restore MongoDB
if ($MongoBackup -and (Test-Path $MongoBackup)) {
    Verify-Checksum $MongoBackup
    Write-Host "Restoring MongoDB from '$MongoBackup' into '$MongoContainer'..." -ForegroundColor Yellow

    if ($MongoBackup.EndsWith(".enc")) {
        if (-not $EncryptionKey) {
            Write-Error "Backup file is encrypted, but -EncryptionKey parameter or BACKUP_ENCRYPTION_KEY environment variable is missing."
            exit 1
        }
        $tempDecrypted = [System.IO.Path]::GetTempFileName()
        try {
            Write-Host "Decrypting snapshot..." -ForegroundColor Yellow
            Decrypt-FilePBKDF2 -InputPath $MongoBackup -OutputPath $tempDecrypted -Password $EncryptionKey
            docker exec -i $MongoContainer mongorestore -u admin -padminpassword --authenticationDatabase=admin --gzip --archive < $tempDecrypted
            Write-Host "MongoDB restore complete." -ForegroundColor Green
        } finally {
            if (Test-Path $tempDecrypted) { Remove-Item $tempDecrypted -Force }
        }
    } else {
        docker exec -i $MongoContainer mongorestore -u admin -padminpassword --authenticationDatabase=admin --gzip --archive < $MongoBackup
        Write-Host "MongoDB restore complete." -ForegroundColor Green
    }
}

Write-Host "Disaster recovery restore process finished." -ForegroundColor Green

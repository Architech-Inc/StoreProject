<#
.SYNOPSIS
    Restores a MySQL or MongoDB snapshot into running containers.
.EXAMPLE
    .\scripts\restore-database.ps1 -MySqlBackupFile "./backups/store_db_v2_20260831.sql" -MongoBackupFile "./backups/mongodb_20260831.archive.gz"
#>
param (
    [Parameter(Mandatory=$false)]
    [string]$MySqlBackupFile,

    [Parameter(Mandatory=$false)]
    [string]$MongoBackupFile,

    [Parameter(Mandatory=$false)]
    [string]$MySqlContainer = "store-mysql",

    [Parameter(Mandatory=$false)]
    [string]$MongoContainer = "store-mongodb",

    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = "store_db_v2"
)

if ($MySqlBackupFile) {
    if (-not (Test-Path $MySqlBackupFile)) {
        Write-Host "MySQL backup file not found: $MySqlBackupFile" -ForegroundColor Red
        exit 1
    }

    Write-Host "Restoring MySQL database '$DatabaseName' from '$MySqlBackupFile'..." -ForegroundColor Cyan
    if ($MySqlBackupFile.EndsWith(".gz")) {
        Write-Host "Decompressing and piping to MySQL..." -ForegroundColor Yellow
        Get-Content $MySqlBackupFile -Raw | docker exec -i $MySqlContainer mysql -u root -prootpassword $DatabaseName
    } else {
        docker exec -i $MySqlContainer mysql -u root -prootpassword $DatabaseName < $MySqlBackupFile
    }
    Write-Host "MySQL restore finished." -ForegroundColor Green
}

if ($MongoBackupFile) {
    if (-not (Test-Path $MongoBackupFile)) {
        Write-Host "MongoDB backup file not found: $MongoBackupFile" -ForegroundColor Red
        exit 1
    }

    Write-Host "Restoring MongoDB from '$MongoBackupFile'..." -ForegroundColor Cyan
    docker exec -i $MongoContainer mongorestore -u admin -padminpassword --authenticationDatabase=admin --gzip --archive < $MongoBackupFile
    Write-Host "MongoDB restore finished." -ForegroundColor Green
}

Write-Host "Disaster recovery restore completed!" -ForegroundColor Green

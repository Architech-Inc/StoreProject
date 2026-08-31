<#
.SYNOPSIS
    Instantly creates a timestamped MySQL and MongoDB snapshot from running containers.
.EXAMPLE
    .\scripts\backup-now.ps1 -OutputDir "./backups"
#>
param (
    [Parameter(Mandatory=$false)]
    [string]$OutputDir = "./backups",

    [Parameter(Mandatory=$false)]
    [string]$MySqlContainer = "store-mysql",

    [Parameter(Mandatory=$false)]
    [string]$MongoContainer = "store-mongodb",

    [Parameter(Mandatory=$false)]
    [string]$DatabaseName = "store_db_v2"
)

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

Write-Host "Creating instant backup snapshot ($timestamp)..." -ForegroundColor Cyan

# 1. MySQL Dump
$mysqlFile = Join-Path $OutputDir "${DatabaseName}_${timestamp}.sql"
$mysqlGz = "$mysqlFile.gz"
Write-Host "Backing up MySQL database '$DatabaseName' from container '$MySqlContainer'..." -ForegroundColor Yellow

docker exec $MySqlContainer mysqldump -u root -prootpassword --single-transaction --quick $DatabaseName > $mysqlFile
if ($LASTEXITCODE -eq 0) {
    Write-Host "MySQL snapshot created: $mysqlFile" -ForegroundColor Green
} else {
    Write-Host "MySQL backup failed!" -ForegroundColor Red
}

# 2. MongoDB Dump
$mongoArchive = Join-Path $OutputDir "mongodb_${timestamp}.archive.gz"
Write-Host "Backing up MongoDB from container '$MongoContainer'..." -ForegroundColor Yellow

docker exec $MongoContainer mongodump -u admin -padminpassword --authenticationDatabase=admin --gzip --archive > $mongoArchive
if ($LASTEXITCODE -eq 0) {
    Write-Host "MongoDB snapshot created: $mongoArchive" -ForegroundColor Green
} else {
    Write-Host "MongoDB backup failed!" -ForegroundColor Red
}

Write-Host "Instant backup complete! All snapshots saved in $OutputDir" -ForegroundColor Green

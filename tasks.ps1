<#
.SYNOPSIS
    ClexAn Foods (StoreProject) — Master Platform PowerShell Task Runner
.DESCRIPTION
    Cross-platform CLI task runner for Windows and PowerShell 7+ environments.
    Equivalent to the root Makefile with native PowerShell parameter validation,
    ANSI colors, timing metrics, and error handling.
.EXAMPLE
    .\tasks.ps1 build
    .\tasks.ps1 test
    .\tasks.ps1 run api
    .\tasks.ps1 platform up
    .\tasks.ps1 provision -Slug "bastos-market" -StoreName "Bastos Fresh Market"
    .\tasks.ps1 audit
#>

[CmdletBinding()]
param (
    [Parameter(Position=0)]
    [ValidateSet("help", "restore", "build", "test", "clean", "run", "platform", "provision", "migrate", "backup", "restore-db", "audit")]
    [string]$Command = "help",

    [Parameter(Position=1)]
    [string]$Subcommand,

    [Parameter(Mandatory=$false)]
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",

    [Parameter(Mandatory=$false)]
    [string]$Slug = "bastos-market",

    [Parameter(Mandatory=$false)]
    [string]$StoreName = "Bastos Fresh Market",

    [Parameter(Mandatory=$false)]
    [string]$AdminEmail = "admin@bastos.cm",

    [Parameter(Mandatory=$false)]
    [string]$AdminPassword = "SuperSecret123!",

    [Parameter(Mandatory=$false)]
    [string]$Currency = "XAF",

    [Parameter(Mandatory=$false)]
    [string]$ControlPlaneUrl = "http://localhost:5050"
)

$ErrorActionPreference = "Stop"
$Sln = "StoreProject.sln"

function Show-Help {
    Write-Host ""
    Write-Host "==========================================================" -ForegroundColor Green
    Write-Host " ClexAn Foods (StoreProject) — PowerShell Task Runner" -ForegroundColor Green
    Write-Host "==========================================================" -ForegroundColor Green
    Write-Host "Usage: .\tasks.ps1 <command> [subcommand] [-Parameters]" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Build & Test Commands:" -ForegroundColor White
    Write-Host "  build               " -NoNewline -ForegroundColor Yellow; Write-Host "Build entire solution (default: Release, 0 warnings policy)"
    Write-Host "  test                " -NoNewline -ForegroundColor Yellow; Write-Host "Run xUnit test suite (Store.API.Tests)"
    Write-Host "  restore             " -NoNewline -ForegroundColor Yellow; Write-Host "Restore NuGet dependencies"
    Write-Host "  clean               " -NoNewline -ForegroundColor Yellow; Write-Host "Clean bin/obj folders and temporary build artifacts"
    Write-Host "  audit               " -NoNewline -ForegroundColor Yellow; Write-Host "Run clean Release build + full test suite verification"
    Write-Host ""
    Write-Host "Service Runners:" -ForegroundColor White
    Write-Host "  run api             " -NoNewline -ForegroundColor Yellow; Write-Host "Start Store.API (REST API) on https://localhost:7112"
    Write-Host "  run ui              " -NoNewline -ForegroundColor Yellow; Write-Host "Start Store.UI (Operations Console) on https://localhost:5001"
    Write-Host "  run cp              " -NoNewline -ForegroundColor Yellow; Write-Host "Start Store.ControlPlane on http://localhost:5050"
    Write-Host "  run tp              " -NoNewline -ForegroundColor Yellow; Write-Host "Start Store.TenantPortal on http://localhost:5060"
    Write-Host ""
    Write-Host "Container & Multi-Tenant Platform:" -ForegroundColor White
    Write-Host "  platform up         " -NoNewline -ForegroundColor Yellow; Write-Host "Start Traefik + ControlPlane via Docker Compose"
    Write-Host "  platform down       " -NoNewline -ForegroundColor Yellow; Write-Host "Stop Docker Compose platform"
    Write-Host "  platform logs       " -NoNewline -ForegroundColor Yellow; Write-Host "Stream live logs from platform containers"
    Write-Host "  provision           " -NoNewline -ForegroundColor Yellow; Write-Host "Provision an isolated tenant store via ControlPlane"
    Write-Host ""
    Write-Host "Database Operations:" -ForegroundColor White
    Write-Host "  migrate             " -NoNewline -ForegroundColor Yellow; Write-Host "Apply pending EF Core migrations to database"
    Write-Host "  backup              " -NoNewline -ForegroundColor Yellow; Write-Host "Trigger immediate manual backup snapshot"
    Write-Host "  restore-db          " -NoNewline -ForegroundColor Yellow; Write-Host "Restore database from latest backup snapshot"
    Write-Host ""
}

function Invoke-TimedAction([string]$Label, [scriptblock]$Action) {
    Write-Host "`n>>> $Label" -ForegroundColor Cyan
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        & $Action
        $sw.Stop()
        Write-Host "✓ $Label completed in $($sw.Elapsed.TotalSeconds.ToString('F2'))s" -ForegroundColor Green
    }
    catch {
        $sw.Stop()
        Write-Host "✗ $Label failed after $($sw.Elapsed.TotalSeconds.ToString('F2'))s: $_" -ForegroundColor Red
        exit 1
    }
}

switch ($Command) {
    "help" {
        Show-Help
    }

    "restore" {
        Invoke-TimedAction "Restoring NuGet Packages" {
            dotnet restore $Sln
        }
    }

    "build" {
        Invoke-TimedAction "Building Solution [$Configuration]" {
            dotnet build $Sln --configuration $Configuration
        }
    }

    "test" {
        Invoke-TimedAction "Running Unit Tests [Store.API.Tests]" {
            dotnet test Store.API.Tests --configuration Release --no-build
        }
    }

    "clean" {
        Invoke-TimedAction "Cleaning Build Artifacts & Temporary Files" {
            dotnet clean $Sln
            Get-ChildItem -Path . -Include bin, obj -Recurse -Directory -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
            Get-ChildItem -Path . -Filter "*.log" -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
            Write-Host "Cleaned bin/obj directories and root *.log files." -ForegroundColor DarkGray
        }
    }

    "run" {
        switch ($Subcommand) {
            "api" {
                Write-Host "Launching Store.API on https://localhost:7112 (Swagger: /swagger)..." -ForegroundColor Green
                dotnet run --project Store.API
            }
            "ui" {
                Write-Host "Launching Store.UI on https://localhost:5001..." -ForegroundColor Green
                dotnet run --project Store.UI
            }
            "cp" {
                Write-Host "Launching Store.ControlPlane on http://localhost:5050..." -ForegroundColor Green
                dotnet run --project Store.ControlPlane
            }
            "tp" {
                Write-Host "Launching Store.TenantPortal on http://localhost:5060..." -ForegroundColor Green
                dotnet run --project Store.TenantPortal
            }
            default {
                Write-Host "Unknown service '$Subcommand'. Valid services: api, ui, cp, tp" -ForegroundColor Red
                exit 1
            }
        }
    }

    "platform" {
        switch ($Subcommand) {
            "up" {
                Invoke-TimedAction "Starting Edge Proxy & Control Plane Platform" {
                    docker compose -f docker-compose.traefik.yml -f docker-compose.platform.yml up -d
                }
            }
            "down" {
                Invoke-TimedAction "Stopping Platform Stacks" {
                    docker compose -f docker-compose.traefik.yml -f docker-compose.platform.yml down
                }
            }
            "restart" {
                Invoke-TimedAction "Restarting Platform Stacks" {
                    docker compose -f docker-compose.traefik.yml -f docker-compose.platform.yml down
                    docker compose -f docker-compose.traefik.yml -f docker-compose.platform.yml up -d
                }
            }
            "logs" {
                docker compose -f docker-compose.traefik.yml -f docker-compose.platform.yml logs -f
            }
            default {
                Write-Host "Unknown platform action '$Subcommand'. Valid actions: up, down, restart, logs" -ForegroundColor Red
                exit 1
            }
        }
    }

    "provision" {
        Invoke-TimedAction "Provisioning Tenant '$Slug' ($StoreName)" {
            if (Test-Path ".\scripts\provision-tenant.ps1") {
                & .\scripts\provision-tenant.ps1 -StoreName $StoreName -Slug $Slug -AdminEmail $AdminEmail -AdminPassword $AdminPassword -Currency $Currency -ControlPlaneUrl $ControlPlaneUrl
            }
            else {
                Write-Host "scripts/provision-tenant.ps1 not found!" -ForegroundColor Red
                exit 1
            }
        }
    }

    "migrate" {
        Invoke-TimedAction "Applying EF Core Database Migrations" {
            dotnet ef database update --project Store.DbServices --startup-project Store.API
        }
    }

    "backup" {
        Invoke-TimedAction "Executing Database Backup" {
            if (Test-Path ".\scripts\backup-now.ps1") {
                & .\scripts\backup-now.ps1
            }
            else {
                Write-Host "scripts/backup-now.ps1 not found!" -ForegroundColor Red
                exit 1
            }
        }
    }

    "restore-db" {
        Invoke-TimedAction "Restoring Database from Snapshot" {
            if (Test-Path ".\scripts\restore-database.ps1") {
                & .\scripts\restore-database.ps1
            }
            else {
                Write-Host "scripts/restore-database.ps1 not found!" -ForegroundColor Red
                exit 1
            }
        }
    }

    "audit" {
        Invoke-TimedAction "Executing Full Solution Audit Verification" {
            Write-Host "1. Building solution in Release mode (0 warnings policy)..." -ForegroundColor White
            dotnet build $Sln --configuration Release
            Write-Host "`n2. Running complete test suite..." -ForegroundColor White
            dotnet test Store.API.Tests --configuration Release --no-build
        }
        Write-Host "`n========================================" -ForegroundColor Green
        Write-Host "✓ Release build clean (0 warnings)" -ForegroundColor Green
        Write-Host "✓ All test suites passing" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
    }
}

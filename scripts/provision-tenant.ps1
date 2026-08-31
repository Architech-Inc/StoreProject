<#
.SYNOPSIS
    Provisions a new isolated tenant store container stack.
.EXAMPLE
    .\scripts\provision-tenant.ps1 -StoreName "Bastos Fresh Market" -Slug "bastos-market" -AdminEmail "admin@bastos.cm" -AdminPassword "SuperSecret123!"
#>
param (
    [Parameter(Mandatory=$true)]
    [string]$StoreName,

    [Parameter(Mandatory=$true)]
    [string]$Slug,

    [Parameter(Mandatory=$true)]
    [string]$AdminEmail,

    [Parameter(Mandatory=$false)]
    [string]$AdminUsername = "admin",

    [Parameter(Mandatory=$true)]
    [string]$AdminPassword,

    [Parameter(Mandatory=$false)]
    [string]$Currency = "XAF",

    [Parameter(Mandatory=$false)]
    [string]$ControlPlaneUrl = "http://localhost:5050"
)

$body = @{
    storeName = $StoreName
    slug = $Slug.ToLowerInvariant()
    adminEmail = $AdminEmail
    adminUsername = $AdminUsername
    adminPassword = $AdminPassword
    currency = $Currency
    planTier = 1 # Professional
} | ConvertTo-Json

Write-Host "Calling Control Plane at $ControlPlaneUrl to provision tenant '$Slug'..." -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri "$ControlPlaneUrl/api/control/tenants/provision" -Method Post -Body $body -ContentType "application/json"
    Write-Host "Tenant provisioned successfully!" -ForegroundColor Green
    Write-Host "UI URL:  $($response.data.uiUrl)" -ForegroundColor Yellow
    Write-Host "API URL: $($response.data.apiUrl)" -ForegroundColor Yellow
    Write-Host "Status:  $($response.data.status)" -ForegroundColor White
}
catch {
    Write-Host "Failed to provision tenant: $_" -ForegroundColor Red
}

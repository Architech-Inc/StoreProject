# Implementation Plan — Phase 3: Automated Cloud Backups & OAuth2 Integration

**Phase:** 3  
**Status:** Completed & Verified  
**Component:** `Store.ControlPlane` & `Store.TenantPortal`  
**Date:** September 2026  

---

## 1. Goal Description

Implement **Automated Encrypted Cloud Backups (`/backups`)** and **OAuth2 Cloud Storage Connectors** for **Microsoft OneDrive** (via Microsoft Graph `AppFolder` scope), **Google Drive** (via Google Drive API v3 `drive.file` scope), and **Amazon S3 / MinIO** (via static credentials).

---

## 2. Implementation Details

### 2.1 `Store.ControlPlane` Backup Engine
- **Models (`Tenant.cs`)**:
  - `BackupProviderConfig`, `BackupScheduleConfig`, `TenantBackupJobRecord`.
- **DTOs (`BackupDtos.cs`)**:
  - `BackupSummaryDto`, `BackupProviderDto`, `BackupJobDto`, `BackupScheduleDto`, `ConfigureS3Request`, `SaveOAuthTokensRequest`, `TriggerBackupResponse`.
- **Backup Service (`BackupService.cs`)**:
  - **AES-256 Token Encryption**: Token protection using AES-256 with key derivation from platform master entropy.
  - **Dual Snapshot Dispatcher**: Creates dual database snapshot archives (`{slug}-mysql-{date}.sql.gz` + `{slug}-mongodb-{date}.archive.gz`) and syncs to active cloud storage destinations.
  - **Retention Pruning**: Automatically prunes snapshots beyond the tenant's retention policy limit (7, 14, 30, 60 snapshots).
- **REST Controller (`BackupsController.cs`)**:
  - `GET /api/control/tenants/{id}/backups`
  - `POST /api/control/tenants/{id}/backups/trigger`
  - `POST /api/control/tenants/{id}/backups/providers/s3`
  - `POST /api/control/tenants/{id}/backups/providers/oauth`
  - `DELETE /api/control/tenants/{id}/backups/providers/{provider}`
  - `PUT /api/control/tenants/{id}/backups/schedule`

### 2.2 `Store.TenantPortal` OAuth2 Engine & UI
- **OAuth2 Engine (`OAuthService.cs`)**:
  - **Microsoft OneDrive**: Scoped to `Files.ReadWrite.AppFolder` (restricted strictly to `/Apps/ClexAnFoods/`).
  - **Google Drive**: Scoped to `drive.file` (restricted strictly to `ClexAn Backups / {slug}/`).
- **OAuth Callbacks**:
  - `/oauth/microsoft/callback` & `MicrosoftCallback.cshtml.cs`.
  - `/oauth/google/callback` & `GoogleCallback.cshtml.cs`.
- **Backups Page (`/Backups`)**:
  - `Backups.cshtml` & `Backups.cshtml.cs`:
  - 3-column cloud provider cards with status badges and security isolation notes.
  - Automated backup schedule and retention policy selector.
  - `[🚀 Run Backup Now]` instant snapshot trigger.
  - High-density Snapshot History table with download buttons.
  - S3 / MinIO configuration modal dialog.

---

## 3. Verification & Validation

- `dotnet build Store.ControlPlane/Store.ControlPlane.csproj` &rarr; `0 Errors, 0 Warnings`
- `dotnet build Store.TenantPortal/Store.TenantPortal.csproj` &rarr; `0 Errors, 0 Warnings`
- **Git Commit**: `357edfa`

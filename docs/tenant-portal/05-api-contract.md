# 05 — Store.TenantPortal API Contract

**Status:** Draft
**Version:** 1.0
**Date:** September 2026

> All new endpoints are additions to `Store.ControlPlane`.
> Base URL: `http://{controlPlaneHost}:9999`
> All responses use the `ApiResponse<T>` envelope: `{ success, message, data }`.

---

## 1. Auth Endpoints

### POST /api/control/auth/register

Register a new portal account.

**Request:**
```json
{
  "fullName":        "Alice Njomo",
  "email":           "alice@acme-foods.com",
  "password":        "Str0ng!Pass#",
  "confirmPassword": "Str0ng!Pass#"
}
```

**Validation:**
- `email`: valid email format, unique across all portal accounts
- `password`: min 8 chars, 1 uppercase, 1 digit, 1 special char
- `confirmPassword`: must match `password`

**Response 201 Created:**
```json
{
  "success": true,
  "message": "Account created. Please complete store setup.",
  "data": {
    "accountId": "guid",
    "email": "alice@acme-foods.com",
    "fullName": "Alice Njomo",
    "sessionToken": "eyJ..."
  }
}
```

**Response 400 (validation errors):**
```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": {
    "email": ["Email is already registered."],
    "password": ["Password must contain at least one special character."]
  }
}
```

---

### POST /api/control/auth/login

Authenticate with an existing portal account.

**Request:**
```json
{
  "email":    "alice@acme-foods.com",
  "password": "Str0ng!Pass#"
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "sessionToken": "eyJ...",
    "tenantId": "guid-or-null",       // null if tenant not yet provisioned
    "tenantName": "Acme Foods Ltd",
    "expiresAt": "2026-09-01T20:00:00Z"
  }
}
```

**Response 401 (invalid credentials):**
```json
{ "success": false, "message": "Invalid email or password." }
```

Note: Same error message for wrong email or wrong password — prevents user enumeration.

---

## 2. Slug Availability

### GET /api/control/slugs/check?slug={slug}

Lightweight check used for real-time slug availability in the onboarding wizard.

**Response 200:**
```json
{ "available": true }
```
or
```json
{ "available": false, "reason": "already taken" }
```
or
```json
{ "available": false, "reason": "reserved by system" }
```

---

## 3. Tenant Lifecycle (Existing — documented for completeness)

| Method | Path | Description |
|:---|:---|:---|
| `POST` | `/api/control/tenants/provision` | Provision silo (existing) |
| `GET` | `/api/control/tenants/{id}` | Get tenant detail + provisioning log |
| `POST` | `/api/control/tenants/{id}/suspend` | Suspend silo |
| `POST` | `/api/control/tenants/{id}/resume` | Resume silo |
| `POST` | `/api/control/tenants/{id}/health` | Force health check |
| `DELETE` | `/api/control/tenants/{id}` | Deprovision silo |

---

## 4. Domain Management Endpoints

### GET /api/control/tenants/{id}/domains

Get the current domain configuration for a tenant.

**Response 200:**
```json
{
  "success": true,
  "data": {
    "tenantId": "guid",
    "slug": "acme-foods",
    "platformUiUrl": "https://acme-foods.store.domain",
    "platformApiUrl": "https://api.acme-foods.store.domain",
    "customDomain": "acme-foods.com",
    "customDomainStatus": "Verified",
    "verificationRecordName": "_clexan-verify.acme-foods.com",
    "verificationRecordValue": "clxv_4a8f3bd9...",
    "customDomainVerifiedAt": "2026-09-01T09:15:00Z"
  }
}
```

`customDomainStatus`: `"NotConfigured"` | `"Pending"` | `"Verified"` | `"Failed"`

---

### POST /api/control/tenants/{id}/domains/custom

Set or update a custom domain. Generates verification token and returns DNS record instructions.

**Request:**
```json
{ "domain": "acme-foods.com" }
```

**Validation:**
- Valid domain format (no protocol, no trailing slash, no path)
- Not in reserved list
- Not already claimed by another tenant

**Response 200:**
```json
{
  "success": true,
  "message": "Custom domain registered. Add the DNS TXT record to verify ownership.",
  "data": {
    "domain": "acme-foods.com",
    "status": "Pending",
    "verificationRecordName": "_clexan-verify.acme-foods.com",
    "verificationRecordValue": "clxv_4a8f3bd9c72e..."
  }
}
```

---

### POST /api/control/tenants/{id}/domains/verify

Trigger a live DNS TXT lookup to verify ownership of the pending custom domain.

**Request:** (no body)

**Response 200 (verified):**
```json
{
  "success": true,
  "message": "Domain verified successfully. Traefik routing updated.",
  "data": {
    "domain": "acme-foods.com",
    "status": "Verified",
    "verifiedAt": "2026-09-01T10:22:00Z"
  }
}
```

**Response 200 (not yet verified):**
```json
{
  "success": false,
  "message": "DNS TXT record not found yet.",
  "data": {
    "domain": "acme-foods.com",
    "status": "Pending",
    "checkedHost": "_clexan-verify.acme-foods.com",
    "expectedValue": "clxv_4a8f3bd9...",
    "foundValues": []
  }
}
```

---

### DELETE /api/control/tenants/{id}/domains/custom

Remove the custom domain. Reverts Traefik routing to platform subdomain only.

**Response 200:**
```json
{ "success": true, "message": "Custom domain removed. Store accessible via platform subdomain." }
```

---

## 5. Branch Management Endpoints

### GET /api/control/tenants/{id}/branches

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "branchId": "guid",
      "branchName": "HQ",
      "branchSlug": "hq",
      "domainType": "Platform",
      "resolvedUrl": "https://hq.acme-foods.store.domain",
      "verificationStatus": "Verified",
      "dateCreated": "2026-09-01T09:00:00Z"
    },
    {
      "branchId": "guid",
      "branchName": "Mfoundi",
      "branchSlug": "mfoundi",
      "domainType": "Custom",
      "resolvedUrl": "https://mfoundi.acme-foods.com",
      "verificationStatus": "Pending",
      "verificationRecordName": "_clexan-verify.mfoundi.acme-foods.com",
      "verificationRecordValue": "clxv_9b2f...",
      "dateCreated": "2026-09-01T11:00:00Z"
    }
  ]
}
```

---

### POST /api/control/tenants/{id}/branches

Add a new branch subdomain.

**Request:**
```json
{
  "branchName":      "HQ",
  "branchSlug":      "hq",
  "domainType":      "Platform",
  "customSubdomain": null
}
```

For custom type:
```json
{
  "branchName":      "Mfoundi",
  "branchSlug":      "mfoundi",
  "domainType":      "Custom",
  "customSubdomain": "mfoundi.acme-foods.com"
}
```

**Response 201 Created:**
```json
{
  "success": true,
  "data": {
    "branchId": "guid",
    "resolvedUrl": "https://hq.acme-foods.store.domain",
    "verificationStatus": "Verified"
  }
}
```

Platform branches are immediately `Verified`. Custom branches are `Pending` until DNS is confirmed.

**Validation errors:**
- `branchSlug` already exists for this tenant
- `customSubdomain` requires custom domain to be verified first
- `branchSlug` contains invalid characters

---

### POST /api/control/tenants/{id}/branches/{branchId}/verify

Trigger DNS TXT verification for a custom branch subdomain.

Same response pattern as `/domains/verify`.

---

### DELETE /api/control/tenants/{id}/branches/{branchId}

Remove a branch mapping. Traefik config updated immediately.

**Response 200:**
```json
{ "success": true, "message": "Branch mapping removed." }
```

---

## 6. Backup Endpoints

### GET /api/control/tenants/{id}/backups/providers

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "providerId": "guid",
      "providerType": "GoogleDrive",
      "isEnabled": true,
      "isConnected": true,
      "targetFolder": "1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgVE2upms",
      "lastBackupAt": "2026-09-01T02:00:00Z",
      "lastBackupSuccess": true
    }
  ]
}
```

Note: OAuth tokens are **never** returned in API responses. Only `isConnected: true/false`.

---

### POST /api/control/tenants/{id}/backups/providers

Add or update a backup provider.

**Request (S3/MinIO — only provider with static creds in request body):**
```json
{
  "providerType":  "S3",
  "endpoint":      "https://minio.yourserver.com",
  "region":        "us-east-1",
  "bucketName":    "store-backups",
  "accessKeyId":   "AKIAIOSFODNN7EXAMPLE",
  "secretAccessKey":"wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY",
  "keyPrefix":     "acme-foods"
}
```

For OneDrive/Google: provider is added via OAuth callback — no body needed here;
the callback handler calls this endpoint internally after token exchange.

**Response 201:**
```json
{ "success": true, "data": { "providerId": "guid" } }
```

---

### POST /api/control/tenants/{id}/backups/schedule

Update backup schedule and retention.

**Request:**
```json
{
  "scheduleCron":    "0 2 * * *",
  "retentionDays":   7
}
```

**Response 200:**
```json
{ "success": true, "message": "Backup schedule updated." }
```

---

### POST /api/control/tenants/{id}/backups/trigger

Trigger an immediate manual backup across all enabled providers.

**Response 202 Accepted:**
```json
{ "success": true, "message": "Backup job queued.", "data": { "jobId": "guid" } }
```

---

### GET /api/control/tenants/{id}/backups/history

**Query params:** `?limit=20&offset=0`

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "jobId": "guid",
      "startedAt": "2026-09-01T02:00:00Z",
      "completedAt": "2026-09-01T02:03:12Z",
      "databaseType": "MySQL",
      "providerType": "GoogleDrive",
      "fileSizeBytes": 471859200,
      "success": true,
      "isManual": false
    }
  ],
  "total": 42
}
```

---

### DELETE /api/control/tenants/{id}/backups/providers/{providerId}

Disconnect / remove a backup provider. Clears OAuth tokens.

**Response 200:**
```json
{ "success": true, "message": "Backup provider disconnected." }
```

---

## 7. OAuth Callback Endpoints (Control Plane)

### GET /api/control/oauth/microsoft/callback

Called by Microsoft after user consents. Handles code exchange and token storage.

**Query params:** `?code={authCode}&state={encryptedTenantId}`

**Internal flow:**
1. Validate `state` (decrypt, verify not tampered, check expiry)
2. Exchange `code` for tokens via POST to Microsoft token endpoint
3. Encrypt refresh token and access token
4. Store in `BackupProvider` record (type: `OneDrive`)
5. Return `text/html` that closes the popup and sends `postMessage` to parent:
   `window.opener.postMessage({ provider: 'OneDrive', connected: true }, '*'); window.close();`

---

### GET /api/control/oauth/google/callback

Same pattern for Google OAuth2.

---

## 8. Error Codes

| HTTP Status | `success` | Meaning |
|:---:|:---:|:---|
| 200 | true | Request successful |
| 201 | true | Resource created |
| 202 | true | Request accepted, processing async |
| 400 | false | Validation error or bad input |
| 401 | false | Not authenticated or invalid session |
| 403 | false | Authenticated but not authorized for this tenant |
| 404 | false | Resource not found |
| 409 | false | Conflict (slug taken, domain already claimed) |
| 500 | false | Internal server error (Control Plane side) |
| 503 | false | Dependency unavailable (Docker daemon, DNS resolver) |

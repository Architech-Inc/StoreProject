# 06 — Store.TenantPortal Security Specification

**Status:** Draft
**Version:** 1.0
**Date:** September 2026

---

## 1. Threat Model

### 1.1 Assets to Protect

| Asset | Sensitivity | Location |
|:---|:---:|:---|
| Tenant portal password (hash + salt) | High | `App_Data/tenants.json` |
| OAuth refresh tokens (OneDrive, Google) | Critical | `App_Data/tenants.json` (AES-256 encrypted) |
| S3/MinIO secret keys | Critical | `App_Data/tenants.json` (AES-256 encrypted) |
| DNS verification tokens | Medium | `App_Data/tenants.json` |
| Tenant database passwords | Critical | `App_Data/tenants.json` (AES-256 encrypted in secrets) |
| JWT signing keys | Critical | Injected as env vars only; never stored in files |
| Custom domain configuration | Low | `App_Data/tenants.json` |
| Backup files in cloud storage | High | Tenant's own cloud account |

### 1.2 Threat Actors

| Actor | Capability | Primary Threat |
|:---|:---|:---|
| Unauthenticated internet user | Public access to portal | Account enumeration, brute force login |
| Authenticated tenant (own silo) | Cookie session | Accessing another tenant's data |
| Malicious tenant | Control Portal access | Slug/domain squatting, SSRF via DNS |
| Compromised OAuth provider | Token interception | OAuth state forgery |
| Supply chain / CDN attack | Script injection | XSS via external scripts |

---

## 2. Authentication Security

### 2.1 Password Hashing

- Algorithm: **PBKDF2-SHA512**
- Iterations: **250,000** (OWASP recommended minimum for PBKDF2-SHA512)
- Salt: **32 random bytes** per account, generated with `RandomNumberGenerator.Fill()`
- Hash length: **64 bytes**
- Storage format: `{iterations}:{base64(salt)}:{base64(hash)}` — future-proof for iteration count changes

```csharp
public static string HashPassword(string password)
{
    var salt = new byte[32];
    RandomNumberGenerator.Fill(salt);
    var hash = Rfc2898DeriveBytes.Pbkdf2(
        password, salt, 250_000, HashAlgorithmName.SHA512, 64);
    return $"250000:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
}

public static bool VerifyPassword(string password, string storedHash)
{
    var parts = storedHash.Split(':');
    var iterations = int.Parse(parts[0]);
    var salt = Convert.FromBase64String(parts[1]);
    var expectedHash = Convert.FromBase64String(parts[2]);
    var actualHash = Rfc2898DeriveBytes.Pbkdf2(
        password, salt, iterations, HashAlgorithmName.SHA512, 64);
    return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
}
```

### 2.2 Login Rate Limiting

Dedicated rate limit policy on auth endpoints:

```csharp
// Program.cs — Control Plane
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("PortalAuth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0
            }));
});
```

Applied to: `POST /api/control/auth/login`, `POST /api/control/auth/register`

### 2.3 Login Timing Attack Mitigation

When login fails (wrong email or wrong password), always perform the password hash comparison
regardless — prevents timing analysis to enumerate valid accounts:

```csharp
// Always compute hash comparison even if account not found
var dummyHash = "250000:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=:...";
var storedHash = account?.PortalPasswordHash ?? dummyHash;
var _ = VerifyPassword(request.Password, storedHash);

if (account == null) return Unauthorized();   // Return after hash comparison
```

### 2.4 Session Cookie Security

```csharp
options.Cookie.HttpOnly = true;               // Not accessible from JavaScript
options.Cookie.SameSite = SameSiteMode.Strict;// Prevents CSRF from other origins
options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;  // HTTPS in production
options.Cookie.Name = "TP_Session";
options.ExpireTimeSpan = TimeSpan.FromHours(8);
options.SlidingExpiration = true;             // Resets 8h on each authenticated request
```

---

## 3. Authorization: Tenant Data Isolation

### 3.1 Tenant-Scoped Authorization Filter

Every page model that accesses tenant data must verify the request's `TenantId` matches
the session's `TenantId`. A `[TenantOwnerOnly]` page filter enforces this:

```csharp
public class TenantOwnerOnlyAttribute : Attribute, IPageFilter
{
    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var httpContext = context.HttpContext;
        var sessionTenantId = httpContext.User.FindFirst("TenantId")?.Value;
        var routeTenantId = context.RouteData.Values["id"]?.ToString();

        if (routeTenantId != null && sessionTenantId != routeTenantId)
        {
            context.Result = new ForbidResult();
        }
    }
}
```

This ensures a tenant cannot access another tenant's domain config, branches, or backups
even by manually crafting a URL with a different `{id}`.

### 3.2 Control Plane Side: Tenant Identity Verification

The Control Plane's extended endpoints require a session token that encodes `TenantId`.
The `[PortalAuth]` attribute validates:
1. Bearer token present in `Authorization` header
2. Token not expired
3. `TenantId` in token matches the `{id}` path parameter

---

## 4. CSRF Protection

- ASP.NET Core's built-in antiforgery (`__RequestVerificationToken`) is applied to all
  `POST`, `PUT`, `DELETE` Razor Page handlers via `app.UseAntiforgery()`.
- AJAX requests from `portal.js` include the antiforgery token in the `X-XSRF-TOKEN` header.
- OAuth `state` parameter: `state = base64url(AesGcm.Encrypt(tenantId + timestamp))`.
  Verified on callback — prevents CSRF attacks on the OAuth flow.

---

## 5. Input Validation & Injection Prevention

### 5.1 Slug Validation

```csharp
[RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug must be lowercase alphanumeric with hyphens only.")]
[StringLength(50, MinimumLength = 3)]
public string Slug { get; set; }
```

No slug is ever passed to a shell command without sanitization. Docker Compose filenames
are written as `Path.Combine(baseDir, slug, "docker-compose.yml")` with slug already
validated — no path traversal possible.

### 5.2 Domain Input Validation

```csharp
private static readonly Regex DomainRegex = new(
    @"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$",
    RegexOptions.Compiled);
```

Domain is never passed to DNS lookup without passing this regex first.

### 5.3 SQL Injection

Not applicable — the Control Plane uses a JSON file store with no SQL database.
The Portal itself makes no database calls.

### 5.4 XSS Prevention

- All Razor output is HTML-encoded by default (`@variable` syntax)
- No `Html.Raw()` calls with user-supplied data
- CSP header applied (see Section 7)

---

## 6. OAuth Security

### 6.1 State Parameter (Anti-CSRF)

```csharp
// Generating state
var statePayload = JsonSerializer.Serialize(new
{
    tenantId = tenantId.ToString(),
    nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)),
    issuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
});
var state = Convert.ToBase64String(AesGcmEncryption.Encrypt(statePayload, _stateKey));
// Store state in server-side session (not cookie) for comparison on callback
```

```csharp
// Validating state on callback
var decrypted = AesGcmEncryption.Decrypt(Convert.FromBase64String(state), _stateKey);
var payload = JsonSerializer.Deserialize<OAuthStatePayload>(decrypted);
if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - payload.IssuedAt > 600)
    return BadRequest("OAuth state expired.");
// Verify payload.TenantId matches session
```

### 6.2 Token Storage

- OAuth tokens are **never** logged
- OAuth tokens are **never** included in API responses
- OAuth tokens are **never** stored in browser cookies or localStorage
- Stored exclusively server-side in `App_Data/tenants.json` AES-256-GCM encrypted
- `isConnected: true/false` is the only token-related field exposed to the portal frontend

### 6.3 Minimal Scopes

| Provider | Scope Requested | Why Minimal |
|:---|:---|:---|
| OneDrive | `Files.ReadWrite.AppFolder offline_access` | App-folder only — cannot touch user's personal files |
| Google Drive | `drive.file` | Only files created by this app — no access to existing files |

---

## 7. HTTP Security Headers (Portal)

Applied in `Program.cs` middleware:

```csharp
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self'; " +
        "style-src 'self' https://fonts.googleapis.com; " +
        "font-src https://fonts.gstatic.com; " +
        "img-src 'self' data:; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none';";
    await next();
});
```

No external CDN scripts — all JS is bundled and served from `wwwroot/js/`. This eliminates
the CDN integrity (SRI) issue that affected the main Store.UI.

---

## 8. Control Plane API Protection

The Control Plane currently has no authentication. The portal accesses it internally.
For the portal integration, two layers of protection are recommended:

### 8.1 Network Isolation (Immediate)

The `store-control-plane` container should **not** be in `proxy-network` (not exposed via Traefik).
Only `store-tenant-portal` (on the same internal Docker network) can reach it.

```yaml
# Control Plane — internal only
store-control-plane:
  networks:
    - portal-internal     # Shared with store-tenant-portal only
  # NOT on proxy-network — never directly internet-accessible
```

### 8.2 Internal API Key (Phase 1 hardening)

Add a simple API key header check on all Control Plane endpoints called by the portal:

```csharp
// Middleware in Control Plane
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.TryGetValue("X-ControlPlane-Key", out var key) ||
        key != configuration["ControlPlane:InternalApiKey"])
    {
        context.Response.StatusCode = 401;
        return;
    }
    await next();
});
```

The key is a 64-byte random secret set via environment variable — never committed to source control.

---

## 9. Secrets Inventory

| Secret | Where Stored | Rotation |
|:---|:---|:---|
| Portal admin password (PBKDF2 hash) | `tenants.json` | User-initiated via `/settings` |
| AES-256 encryption key | Environment variable only | Manual re-encrypt procedure on rotation |
| OAuth client secrets (Microsoft, Google) | Environment variable only | Per provider's console |
| Internal API key (Portal → ControlPlane) | Environment variable only | Manual rotation |
| DNS verification tokens | `tenants.json` (plaintext — low sensitivity) | Regenerated if domain removed and re-added |
| Tenant DB passwords, JWT keys | `tenants.json` (AES-256 encrypted) | Not rotated post-provision (future: rotation endpoint) |

---

## 10. Audit Logging

All portal authentication and mutation events are logged by the Control Plane:

| Event | Log Level | Fields |
|:---|:---|:---|
| Register | Info | email, timestamp, IP |
| Login success | Info | tenantId, email, timestamp, IP |
| Login failure | Warning | email, timestamp, IP, reason |
| Custom domain set | Info | tenantId, domain, timestamp |
| Domain verified | Info | tenantId, domain, verifiedAt |
| Branch added | Info | tenantId, branchSlug, domainType |
| Branch removed | Info | tenantId, branchId |
| Backup provider connected | Info | tenantId, providerType |
| Backup provider disconnected | Info | tenantId, providerType |
| Manual backup triggered | Info | tenantId, triggeredBy, jobId |
| Silo suspended | Warning | tenantId, by, timestamp |
| Silo deprovisioned | Warning | tenantId, by, timestamp |

Audit logs are written to the ASP.NET Core structured logger (JSON format), captured by
the existing 90-day log retention worker.

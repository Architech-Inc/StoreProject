# 03 — Store.TenantPortal Domain & Routing Specification

**Status:** Draft
**Version:** 1.0
**Date:** September 2026

---

## 1. Overview

This document defines the complete domain routing architecture for the multi-tenant portal,
covering three routing layers:

1. **Platform subdomains** — provisioned automatically, zero tenant DNS effort
2. **Custom domains (BYOD)** — tenant brings their own domain, verified via DNS TXT challenge
3. **Branch sub-subdomains** — per-branch subdomain mapping (platform or custom)

All routing is handled by **Traefik v3** acting as the reverse proxy. The Control Plane
manages Traefik configuration via the **file provider** — writing per-tenant YAML files
to a watched directory that Traefik hot-reloads.

---

## 2. Platform Subdomain Architecture

### 2.1 URL Scheme

Every provisioned tenant automatically receives two platform subdomains:

| Component | Pattern | Example |
|:---|:---|:---|
| Tenant Store (UI) | `https://{slug}.store.{rootDomain}` | `https://acme-foods.store.yourcompany.com` |
| Tenant API | `https://api.{slug}.store.{rootDomain}` | `https://api.acme-foods.store.yourcompany.com` |
| Branch (Platform) | `https://{branch}.{slug}.store.{rootDomain}` | `https://hq.acme-foods.store.yourcompany.com` |

### 2.2 DNS Wildcard Configuration (One-time Setup)

The root domain requires two wildcard DNS records, set up once by the platform operator:

```
Type:   A
Name:   *.store.yourcompany.com
Value:  <server-IP>
TTL:    300

Type:   A
Name:   *.*.store.yourcompany.com        (wildcard for branch sub-subdomains)
Value:  <server-IP>
TTL:    300
```

> **Note:** Many DNS providers do not support multi-level wildcards (`*.*.`).
> In that case, branch platform subdomains must be registered as explicit CNAME records
> by the Control Plane's DNS provider API (e.g. Cloudflare API, Namecheap API).
> The recommended solution for self-hosted deployments is to use `nip.io` which supports
> unlimited subdomain levels.

### 2.3 Traefik Docker Label Routing (From Compose Template)

The tenant silo compose template already contains Traefik labels for platform routing.
These are written at provisioning time and do not change:

```yaml
# {slug}-api labels
- "traefik.http.routers.{slug}-api.rule=Host(`api.{slug}.{rootDomain}`)"
- "traefik.http.services.{slug}-api.loadbalancer.server.port=8080"

# {slug}-ui labels
- "traefik.http.routers.{slug}-ui.rule=Host(`{slug}.{rootDomain}`)"
- "traefik.http.services.{slug}-ui.loadbalancer.server.port=8080"
```

---

## 3. Custom Domain (BYOD) Architecture

### 3.1 Flow Overview

```
Tenant enters domain        Control Plane generates        Tenant adds DNS TXT
in Portal ─────────────►    verification token ─────────►  _clexan-verify.domain.com
                                                           = "clxv_{token}"

Tenant clicks [Check DNS]   Control Plane resolves         SUCCESS: Traefik config
───────────────────────►    DNS TXT record ────────────►   written for custom domain
                            Compares to stored token
```

### 3.2 DNS TXT Verification Token Format

```
Token prefix:   clxv_
Token body:     64 cryptographically random hex bytes
Full value:     clxv_4a8f3bd92e1c7f5a...  (132 chars total)

DNS Record:
  Type:   TXT
  Name:   _clexan-verify.{custom-domain}
  Value:  clxv_{token}
  TTL:    300 (recommended)
```

Token is generated using `RandomNumberGenerator.GetHexString(64)` (or equivalent) and stored
in `Tenant.CustomDomainVerificationToken`. It never expires unless the tenant removes the domain.

### 3.3 DNS TXT Verification Implementation

```csharp
// Store.ControlPlane/Services/DomainVerificationService.cs

public class DomainVerificationService : IDomainVerificationService
{
    private readonly ILookupClient _dnsClient;     // DnsClient NuGet package

    public async Task<DomainVerificationResult> VerifyAsync(
        string domain,
        string expectedToken,
        CancellationToken ct = default)
    {
        var verificationHost = $"_clexan-verify.{domain}";

        try
        {
            var result = await _dnsClient.QueryAsync(
                verificationHost,
                QueryType.TXT,
                cancellationToken: ct);

            var txtRecords = result.Answers
                .OfType<TxtRecord>()
                .SelectMany(r => r.Text)
                .ToList();

            var isVerified = txtRecords.Any(v =>
                string.Equals(v.Trim(), expectedToken, StringComparison.Ordinal));

            return new DomainVerificationResult
            {
                IsVerified = isVerified,
                FoundValues = txtRecords,
                ExpectedValue = expectedToken,
                CheckedHost = verificationHost,
                CheckedAt = DateTime.UtcNow
            };
        }
        catch (DnsResponseException ex)
        {
            return new DomainVerificationResult
            {
                IsVerified = false,
                ErrorMessage = ex.Message,
                CheckedHost = verificationHost,
                CheckedAt = DateTime.UtcNow
            };
        }
    }
}
```

### 3.4 Post-Verification: Traefik Dynamic Config Update

On successful verification, `TraefikDynamicConfigWriter` writes or overwrites the tenant's
Traefik file provider config:

```csharp
// Store.ControlPlane/Services/TraefikDynamicConfigWriter.cs

public class TraefikDynamicConfigWriter : ITraefikDynamicConfigWriter
{
    private readonly string _dynamicConfigDir;  // "/traefik/dynamic/"

    public async Task WriteAsync(Tenant tenant, CancellationToken ct = default)
    {
        var hostRules = BuildHostRules(tenant);
        var yaml = RenderYaml(tenant.Slug, hostRules);
        var filePath = Path.Combine(_dynamicConfigDir, $"{tenant.Slug}.yml");
        await File.WriteAllTextAsync(filePath, yaml, ct);
    }

    private static string BuildHostRules(Tenant tenant)
    {
        var rules = new List<string>
        {
            // Platform subdomain always present (redundant with Docker labels but safe)
            $"Host(`{tenant.UiUrl.Replace("https://","").Replace("http://","")}`)"
        };

        if (!string.IsNullOrEmpty(tenant.CustomDomain) &&
            tenant.CustomDomainStatus == DomainVerificationStatus.Verified)
        {
            rules.Add($"Host(`{tenant.CustomDomain}`)");
        }

        foreach (var branch in tenant.Branches.Where(b =>
            b.VerificationStatus == DomainVerificationStatus.Verified))
        {
            rules.Add($"Host(`{branch.ResolvedUrl.Replace("https://","").Replace("http://","")}`)");
        }

        return string.Join(" || ", rules);
    }

    private static string RenderYaml(string slug, string hostRules) => $@"# Auto-generated by Store.ControlPlane — DO NOT EDIT MANUALLY
# Regenerated on every domain/branch change for tenant: {slug}
http:
  routers:
    {slug}-ui-dynamic:
      rule: ""{hostRules}""
      service: {slug}-ui-svc
      tls:
        certResolver: letsencrypt
  services:
    {slug}-ui-svc:
      loadBalancer:
        servers:
          - url: ""http://{slug}-ui:8080""
";
}
```

### 3.5 Traefik File Provider Configuration (One-time Platform Setup)

Traefik must be configured to watch the dynamic directory:

```yaml
# traefik.yml (static config)
providers:
  docker:
    watch: true
    network: proxy-network
  file:
    directory: /traefik/dynamic   # Watch this directory
    watch: true                   # Hot-reload on file changes
```

The `/traefik/dynamic` directory is a **named Docker volume** (`traefik-dynamic`) shared
between the `store-traefik` container and the `store-control-plane` container:

```yaml
# In docker-compose for Traefik:
  traefik:
    volumes:
      - traefik-dynamic:/traefik/dynamic

# In docker-compose for ControlPlane:
  store-control-plane:
    volumes:
      - traefik-dynamic:/traefik/dynamic
```

### 3.6 Custom Domain Removal

On `DELETE /api/control/tenants/{id}/domains/custom`:
1. Clear `Tenant.CustomDomain`, `CustomDomainStatus`, `CustomDomainVerifiedAt`, `CustomDomainVerificationToken`
2. Call `TraefikDynamicConfigWriter.WriteAsync(tenant)` — rewrites the file without the custom domain rule
3. Tenant store continues to work on platform subdomain

---

## 4. Branch Subdomain Architecture

### 4.1 Branch Subdomain Types

| Type | Pattern | DNS Required by Tenant | Routing Target |
|:---|:---|:---:|:---|
| Platform | `{branch}.{slug}.store.{rootDomain}` | None | Existing `{slug}-ui` container |
| Custom (on BYOD) | `{branch}.{customDomain}` | CNAME record | Existing `{slug}-ui` container |

**Key insight:** All branch subdomains resolve to the **same `{slug}-ui` container**.
The Store.UI application already supports multi-branch via branch context (session/cookie/DB lookup).
Branch subdomains are purely a **routing alias** — no separate container is needed per branch.

### 4.2 Platform Branch Subdomain Registration

For platform branches:

1. The branch URL `{branch}.{slug}.store.{rootDomain}` is covered by the wildcard DNS `*.*.store.{rootDomain}`.
2. Traefik needs a router rule for this specific host.
3. `TraefikDynamicConfigWriter.WriteAsync(tenant)` is called whenever a branch is added or removed —
   it regenerates the full dynamic config for the tenant including all active branch rules.

### 4.3 Custom Branch Subdomain: DNS CNAME Required

For a custom branch like `hq.acme-foods.com`:

The tenant must add at their DNS registrar:

```
Type:   CNAME
Name:   hq.acme-foods.com
Value:  acme-foods.store.yourcompany.com   (or the server IP as an A record)
TTL:    300
```

The portal shows these exact instructions in the Branch drawer when `Custom` type is selected.

Verification for custom branches uses the same DNS TXT challenge pattern as the custom domain:

```
Type:   TXT
Name:   _clexan-verify.hq.acme-foods.com
Value:  clxv_{branchVerificationToken}
TTL:    300
```

### 4.4 Branch Data Model

```csharp
public class AddBranchRequest
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string BranchName { get; set; } = string.Empty;

    [Required, RegularExpression(@"^[a-z0-9-]+$")]
    [StringLength(30, MinimumLength = 2)]
    public string BranchSlug { get; set; } = string.Empty;

    public BranchDomainType DomainType { get; set; } = BranchDomainType.Platform;

    // Only required when DomainType == Custom
    [StringLength(253)]
    public string? CustomSubdomain { get; set; }
}
```

### 4.5 Branch Resolution URL Logic

| Domain Type | Has Custom Domain | Branch Resolved URL |
|:---|:---:|:---|
| Platform | — | `https://{branch}.{slug}.store.{rootDomain}` |
| Custom | Yes, Verified | `https://{branch}.{customDomain}` |
| Custom | No | Error — requires verified custom domain first |

---

## 5. TLS / HTTPS

All custom domains and branches receive automatic TLS via Traefik's built-in ACME integration
with Let's Encrypt:

```yaml
# traefik.yml (static config)
certificatesResolvers:
  letsencrypt:
    acme:
      email: certs@yourcompany.com
      storage: /letsencrypt/acme.json
      httpChallenge:
        entryPoint: web
```

When `certResolver: letsencrypt` is set in the dynamic router config, Traefik automatically
requests and renews certificates for all `Host()` rules in that router.

---

## 6. Edge Cases & Error Conditions

| Scenario | Handling |
|:---|:---|
| Tenant enters a reserved domain (e.g. `admin.com`) | Blocked by domain input validation — no processing |
| DNS TXT record not yet propagated | Show "Not found yet — DNS changes can take up to 48 hours" with retry button |
| Same custom domain claimed by two tenants | First-come-first-served; `SlugExistsAsync`-style uniqueness check on domain |
| Custom domain removed while branches still reference it | Branch URLs on that domain marked `Failed`; shown as warning on Branches page |
| Traefik dynamic config directory not mounted | `TraefikDynamicConfigWriter` throws `InvalidOperationException`; provisioning log records failure |
| Let's Encrypt rate limit hit | Traefik handles retry backoff; portal shows "TLS certificate pending" status |

---

## 7. Traefik Dynamic Config File Lifecycle

| Event | File Written | Content |
|:---|:---|:---|
| Tenant provisioned | `{slug}.yml` | Platform subdomain router only |
| Custom domain verified | `{slug}.yml` | Platform + custom domain router rules |
| Custom domain removed | `{slug}.yml` | Platform only (reverts) |
| Branch added (platform) | `{slug}.yml` | All rules + new branch Host() added |
| Branch added (custom, verified) | `{slug}.yml` | All rules + custom branch Host() added |
| Branch removed | `{slug}.yml` | All rules minus removed branch |
| Tenant deprovisioned | `{slug}.yml` deleted | Traefik removes all routes for that tenant |

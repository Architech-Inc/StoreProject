using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Store.ControlPlane.Data;
using Store.ControlPlane.Models;
using Store.Models.Billing;
using Store.Models.DTOs.Tenant;

namespace Store.ControlPlane.Services;

/// <summary>
/// MT-04 — Implementation of tenant-scoped custom SMTP management, credential rotation, and diagnostic testing.
/// </summary>
public class TenantSmtpService : ITenantSmtpService
{
    private readonly ControlPlaneDbContext _db;
    private readonly ILogger<TenantSmtpService> _logger;

    public TenantSmtpService(
        ControlPlaneDbContext db,
        ILogger<TenantSmtpService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<TenantSmtpConfigDto?> GetSmtpConfigAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.TenantId == tenantId, ct);
        if (tenant == null) return null;

        return MapToDto(tenant);
    }

    public async Task<TenantSmtpConfigDto> UpdateSmtpConfigAsync(Guid tenantId, UpdateTenantSmtpRequest request, CancellationToken ct = default)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.TenantId == tenantId, ct);
        if (tenant == null)
            throw new KeyNotFoundException($"Tenant {tenantId} not found.");

        // Enterprise tier quota gate
        if (!PlanCatalog.IsFeatureEnabled(tenant.PlanTier, PlanFeature.CustomSmtp))
        {
            throw new InvalidOperationException("Custom SMTP Relay is an Enterprise plan feature. Please upgrade your subscription tier to enable custom mail servers.");
        }

        tenant.Secrets.SmtpHost = request.Host.Trim();
        tenant.Secrets.SmtpPort = request.Port;
        tenant.Secrets.SmtpUsername = request.Username?.Trim() ?? string.Empty;

        // Only overwrite password if a non-empty value was provided (allows rotating other fields without re-supplying secret)
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            tenant.Secrets.SmtpPassword = request.Password;
        }

        tenant.Secrets.SmtpFromEmail = request.FromEmail.Trim();
        tenant.Secrets.SmtpFromName = request.FromName?.Trim() ?? string.Empty;
        tenant.Secrets.SmtpEnableSsl = request.EnableSsl;
        tenant.Secrets.SmtpIsEnabled = request.IsEnabled;

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Updated SMTP configuration for tenant {TenantSlug} ({TenantId})", tenant.Slug, tenant.TenantId);

        return MapToDto(tenant);
    }

    public async Task<TestSmtpResponse> TestSmtpConnectionAsync(Guid tenantId, TestSmtpRequest request, CancellationToken ct = default)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.TenantId == tenantId, ct);
        if (tenant == null)
            throw new KeyNotFoundException($"Tenant {tenantId} not found.");

        if (!PlanCatalog.IsFeatureEnabled(tenant.PlanTier, PlanFeature.CustomSmtp))
        {
            throw new InvalidOperationException("Custom SMTP Relay is an Enterprise plan feature.");
        }

        var host = tenant.Secrets.SmtpHost;
        var port = tenant.Secrets.SmtpPort;
        var username = tenant.Secrets.SmtpUsername;
        var password = tenant.Secrets.SmtpPassword;
        var fromEmail = string.IsNullOrWhiteSpace(tenant.Secrets.SmtpFromEmail) ? "noreply@clexanfoods.cm" : tenant.Secrets.SmtpFromEmail;
        var fromName = string.IsNullOrWhiteSpace(tenant.Secrets.SmtpFromName) ? $"{tenant.Name} Relay Test" : tenant.Secrets.SmtpFromName;
        var enableSsl = tenant.Secrets.SmtpEnableSsl;

        if (string.IsNullOrWhiteSpace(host))
        {
            return new TestSmtpResponse(false, "SMTP Host is not configured.", 0);
        }

        var sw = Stopwatch.StartNew();
        bool success = false;
        string message;

        try
        {
            _logger.LogInformation("Executing SMTP connection test for tenant {Slug} via {Host}:{Port} to {Recipient}", tenant.Slug, host, port, request.RecipientEmail);

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                client.Credentials = new NetworkCredential(username, password);
            }

            using var mail = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = $"ClexAn Foods — SMTP Relay Verification ({tenant.Name})",
                Body = $"Hello,\r\n\r\nThis is an automated verification message dispatched from ClexAn Foods SaaS platform.\r\n\r\n" +
                       $"Tenant: {tenant.Name} ({tenant.Slug})\r\n" +
                       $"Timestamp: {DateTime.UtcNow:u}\r\n" +
                       $"SMTP Server: {host}:{port}\r\n" +
                       $"Encryption: {(enableSsl ? "TLS/SSL Enabled" : "Plaintext")}\r\n" +
                       $"From Envelope: {fromEmail}\r\n\r\n" +
                       "If you received this message, your custom email relay is functioning with full delivery readiness.\r\n\r\n" +
                       "Best regards,\r\nClexAn Foods SaaS Platform"
            };

            mail.To.Add(request.RecipientEmail.Trim());

            await client.SendMailAsync(mail, ct);
            sw.Stop();
            success = true;
            message = $"Test email sent successfully to {request.RecipientEmail} in {sw.ElapsedMilliseconds} ms.";
        }
        catch (Exception ex)
        {
            sw.Stop();
            success = false;
            message = $"SMTP connection failed: {ex.Message}";
            _logger.LogWarning(ex, "SMTP connection test failed for tenant {Slug} via {Host}:{Port}", tenant.Slug, host, port);
        }

        tenant.Secrets.SmtpLastTestedAt = DateTime.UtcNow;
        tenant.Secrets.SmtpLastTestStatus = success ? "Success" : message;
        await _db.SaveChangesAsync(ct);

        return new TestSmtpResponse(success, message, sw.ElapsedMilliseconds);
    }

    public async Task<bool> ResetSmtpConfigAsync(Guid tenantId, CancellationToken ct = default)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.TenantId == tenantId, ct);
        if (tenant == null) return false;

        tenant.Secrets.SmtpHost = string.Empty;
        tenant.Secrets.SmtpPort = 587;
        tenant.Secrets.SmtpUsername = string.Empty;
        tenant.Secrets.SmtpPassword = string.Empty;
        tenant.Secrets.SmtpFromEmail = string.Empty;
        tenant.Secrets.SmtpFromName = string.Empty;
        tenant.Secrets.SmtpEnableSsl = true;
        tenant.Secrets.SmtpIsEnabled = false;
        tenant.Secrets.SmtpLastTestedAt = null;
        tenant.Secrets.SmtpLastTestStatus = null;

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Reset custom SMTP configuration to system defaults for tenant {TenantSlug}", tenant.Slug);

        return true;
    }

    private static TenantSmtpConfigDto MapToDto(Tenant t)
    {
        var hasPassword = !string.IsNullOrEmpty(t.Secrets.SmtpPassword);
        return new TenantSmtpConfigDto(
            TenantId: t.TenantId,
            Slug: t.Slug,
            Host: t.Secrets.SmtpHost,
            Port: t.Secrets.SmtpPort,
            Username: t.Secrets.SmtpUsername,
            PasswordMasked: hasPassword ? "••••••••" : null,
            HasPassword: hasPassword,
            FromEmail: t.Secrets.SmtpFromEmail,
            FromName: t.Secrets.SmtpFromName,
            EnableSsl: t.Secrets.SmtpEnableSsl,
            IsEnabled: t.Secrets.SmtpIsEnabled,
            LastTestedAt: t.Secrets.SmtpLastTestedAt,
            LastTestStatus: t.Secrets.SmtpLastTestStatus
        );
    }
}

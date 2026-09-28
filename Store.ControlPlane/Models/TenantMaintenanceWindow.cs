namespace Store.ControlPlane.Models;

/// <summary>
/// MT-07 — a scheduled (or in-progress) maintenance window for a tenant.
///
/// Operators publish these to warn tenants before / during planned outages.
/// The Tenant Portal <c>/Status</c> page shows active + upcoming windows
/// to visitors without authentication.
///
/// Stored as a JSON-serialized collection on <see cref="Tenant.MaintenanceWindows"/>.
/// </summary>
public class MaintenanceWindow
{
    public Guid MaintenanceWindowId { get; set; } = Guid.NewGuid();

    /// <summary>Short title, e.g. "Database upgrade" or "Network maintenance".</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Optional longer description of impact / scope.</summary>
    public string? Description { get; set; }

    /// <summary>Start of the maintenance window (UTC).</summary>
    public DateTime StartUtc { get; set; }

    /// <summary>End of the maintenance window (UTC). Inclusive — service is expected to be back by this time.</summary>
    public DateTime EndUtc { get; set; }

    /// <summary>Severity — drives the badge color on the public status page.</summary>
    public MaintenanceSeverity Severity { get; set; } = MaintenanceSeverity.Info;

    /// <summary>True when an operator marks the window as completed (informational only).</summary>
    public bool IsResolved { get; set; }

    /// <summary>Who scheduled this — operator email or "system".</summary>
    public string CreatedBy { get; set; } = "system";

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
}

public enum MaintenanceSeverity
{
    Info = 0,
    Warning = 1,
    Critical = 2
}
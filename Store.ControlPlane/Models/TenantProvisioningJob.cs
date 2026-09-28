using TenantTier = Store.Models.Billing.TenantTier;

namespace Store.ControlPlane.Models;

/// <summary>
/// MT-01 — Tracks the asynchronous lifecycle of a tenant provisioning request.
///
/// <para>
/// The portal signup → onboarding flow can take 30–90 seconds end-to-end (DNS
/// pre-checks, docker-compose generation, image pull, container bring-up, TLS
/// handshake). Doing all that synchronously blocks the user-facing request
/// thread and gives them no feedback if the network is slow.
///
/// </para>
/// <para>
/// Instead we:
///   1. Persist a <see cref="TenantProvisioningJob"/> row with Status=Pending.
///   2. Return its <c>JobId</c> to the portal immediately so the UI can poll.
///   3. The <see cref="Store.ControlPlane.Services.TenantProvisioningHostedService"/>
///      dequeues the job, runs the existing <c>ProvisionTenantAsync</c> pipeline,
///      and flips the Status to Completed (with TenantId set) or Failed (with error).
/// </para>
/// </summary>
public class TenantProvisioningJob
{
    public Guid JobId { get; set; } = Guid.NewGuid();

    /// <summary>The portal account that owns this job. Used to link Account → Tenant on success.</summary>
    public Guid AccountId { get; set; }

    /// <summary>The user-submitted provisioning payload (mirrors ProvisionTenantRequest).</summary>
    public string StoreName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminUsername { get; set; } = string.Empty;
    public string AdminPasswordCipher { get; set; } = string.Empty;  // AES-GCM under ControlPlane:MasterEncryptionKey
    public string Currency { get; set; } = "XAF";
    public TenantTier PlanTier { get; set; } = TenantTier.Professional;
    public string? CustomDomain { get; set; }

    public TenantProvisioningStatus Status { get; set; } = TenantProvisioningStatus.Pending;
    public string? StatusDetail { get; set; }
    public string? FailureReason { get; set; }

    /// <summary>Set when Status=Completed; the freshly-provisioned tenant's id.</summary>
    public Guid? TenantId { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public enum TenantProvisioningStatus
{
    Pending = 0,    // queued, not started
    InProgress = 1, // hosted service picked it up
    Completed = 2,  // tenant provisioned + account linked
    Failed = 3      // see FailureReason
}

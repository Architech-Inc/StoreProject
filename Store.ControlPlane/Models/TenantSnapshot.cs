namespace Store.ControlPlane.Models;

public class TenantSnapshot
{
    public Guid SnapshotId { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid? ReleaseId { get; set; } // The release version running when the snapshot was taken
    public SnapshotType Type { get; set; } = SnapshotType.PreUpgrade;
    public string SqlDumpPath { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long SizeBytes { get; set; }
}

public enum SnapshotType
{
    PreUpgrade = 0,
    Manual = 1,
    SandboxClone = 2
}

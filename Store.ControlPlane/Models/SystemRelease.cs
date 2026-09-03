namespace Store.ControlPlane.Models;

public class SystemRelease
{
    public Guid ReleaseId { get; set; } = Guid.NewGuid();
    public string VersionName { get; set; } = string.Empty; // e.g. "StoreOS 2.1"
    public string ApiImageTag { get; set; } = "latest";
    public string UiImageTag { get; set; } = "latest";
    public string? DatabaseMigrationTag { get; set; } // Optional: minimum required EF migration
    public DateTime ReleaseDate { get; set; } = DateTime.UtcNow;
    public bool IsPublic { get; set; } = false;
    public string ReleaseNotes { get; set; } = string.Empty;
}

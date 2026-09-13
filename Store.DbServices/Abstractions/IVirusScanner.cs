namespace Store.DbServices.Abstractions;

/// <summary>
/// Antivirus scanning abstraction. Production uses the ClamAV sidecar
/// (<c>clamav-rest</c>) declared in <c>docker-compose.platform.yml</c>.
/// </summary>
public interface IVirusScanner
{
    /// <summary>
    /// Scan a stream for malware. Returns a result that says whether the file
    /// is clean, infected (with signature name), or whether the scan failed
    /// (e.g., scanner unreachable).
    /// </summary>
    Task<ScanResult> ScanAsync(Stream stream, string fileName, CancellationToken ct = default);
}

public record ScanResult(bool IsClean, string? Threat, string? Details)
{
    public static ScanResult Clean() => new(true, null, null);
    public static ScanResult Infected(string threat, string? details = null) =>
        new(false, threat, details);
    public static ScanResult Failed(string details) =>
        new(false, null, details);
}

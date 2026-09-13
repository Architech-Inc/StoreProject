using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Store.DbServices.Abstractions;

namespace Store.DbServices.Services;

/// <summary>
/// ClamAV sidecar implementation of <see cref="IVirusScanner"/>.
/// Hits the REST endpoint exposed by <c>nikolaik/clamav-rest</c> at
/// <c>POST /scan</c> with multipart form-data.
/// </summary>
public class ClamAvVirusScanner : IVirusScanner
{
    private readonly HttpClient _http;
    private readonly ILogger<ClamAvVirusScanner> _logger;
    private readonly string _endpoint;

    public ClamAvVirusScanner(HttpClient http, IConfiguration config, ILogger<ClamAvVirusScanner> logger)
    {
        _http = http;
        _logger = logger;

        var baseUrl = config["Antivirus:ClamAV:BaseUrl"] ?? "http://clamav-rest:8080";
        _endpoint = baseUrl.TrimEnd('/') + "/scan";

        _http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        _http.Timeout = TimeSpan.FromSeconds(60);
    }

    public async Task<ScanResult> ScanAsync(Stream stream, string fileName, CancellationToken ct = default)
    {
        try
        {
            using var form = new MultipartFormDataContent("----scan-" + Guid.NewGuid().ToString("N"));
            var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(content, "file", fileName);

            using var resp = await _http.PostAsync("scan", form, ct);
            var raw = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("ClamAV scanner returned {StatusCode} for {FileName}: {Body}",
                    resp.StatusCode, fileName, raw);
                return ScanResult.Failed($"ClamAV HTTP {(int)resp.StatusCode}");
            }

            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            // nikolaik/clamav-rest response:
            //   { "data": { "result": "clean"|"infected", "malware": "..." }, "success": true }
            var result = root.GetProperty("data").GetProperty("result").GetString();
            if (string.Equals(result, "clean", StringComparison.OrdinalIgnoreCase))
            {
                return ScanResult.Clean();
            }

            if (string.Equals(result, "infected", StringComparison.OrdinalIgnoreCase))
            {
                string? malware = null;
                if (root.GetProperty("data").TryGetProperty("malware", out var m) && m.ValueKind == JsonValueKind.String)
                    malware = m.GetString();
                return ScanResult.Infected(malware ?? "unknown", raw);
            }

            return ScanResult.Failed($"ClamAV returned unrecognised result: {result}");
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return ScanResult.Failed("ClamAV timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ClamAV scan failed for {FileName}", fileName);
            return ScanResult.Failed(ex.Message);
        }
    }
}

/// <summary>
/// Permissive scanner for local development where no ClamAV sidecar is available.
/// Reports every upload as clean. DO NOT use in production.
/// </summary>
public class NoOpVirusScanner : IVirusScanner
{
    private readonly ILogger<NoOpVirusScanner> _logger;

    public NoOpVirusScanner(ILogger<NoOpVirusScanner> logger) => _logger = logger;

    public Task<ScanResult> ScanAsync(Stream stream, string fileName, CancellationToken ct = default)
    {
        _logger.LogWarning("NoOpVirusScanner in use — uploads are NOT being scanned. Set Antivirus:Provider=ClamAV in production.");
        return Task.FromResult(ScanResult.Clean());
    }
}

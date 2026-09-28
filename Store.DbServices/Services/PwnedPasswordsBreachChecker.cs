using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

/// <summary>
/// SEC-27 — HaveIBeenPwned k-anonymity breach checker.
///
/// The endpoint <c>https://api.pwnedpasswords.com/range/{first5}</c> returns
/// ~500 lines of <c>{suffix35}:{count}</c> pairs. We compare the suffix of
/// our SHA-1 hash against that list. The full password NEVER leaves the
/// process — only the first 5 hex chars of its SHA-1 are sent.
///
/// Fails open on any error: network outage, non-2xx, malformed body,
/// timeout. The rest of the password policy still applies.
///
/// Reference: https://haveibeenpwned.com/API/v3#PwnedPasswords
/// </summary>
public class PwnedPasswordsBreachChecker : IBreachChecker
{
    public const string DefaultEndpoint = "https://api.pwnedpasswords.com/range/";

    private readonly HttpClient _http;
    private readonly ILogger<PwnedPasswordsBreachChecker> _logger;
    private readonly string _endpoint;

    public PwnedPasswordsBreachChecker(
        HttpClient http,
        ILogger<PwnedPasswordsBreachChecker> logger,
        string endpoint = DefaultEndpoint)
    {
        _http = http;
        _logger = logger;
        _endpoint = endpoint.TrimEnd('/') + "/";

        // Required by HIBP ToS.
        if (_http.DefaultRequestHeaders.UserAgent.Count == 0)
        {
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("StoreProject/1.0 (HIBP-Auditor)");
        }

        // Pad the request so HIBP can't fingerprint our traffic.
        _http.DefaultRequestHeaders.Add("Add-Padding", "true");
    }

    public async Task<bool> IsBreachedAsync(string password, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(password)) return false;

        var hash = ComputeSha1Hex(password);
        var prefix = hash[..5];
        var suffix = hash[5..];

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(3)); // fail fast

            var url = _endpoint + prefix;
            var body = await _http.GetStringAsync(url, cts.Token);
            return body.Split('\n')
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .Select(line => line.Split(':'))
                .Any(parts => parts.Length == 2 &&
                              string.Equals(parts[0], suffix, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "HIBP breach check failed; failing open for password.");
            return false;
        }
    }

    private static string ComputeSha1Hex(string input)
    {
        var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("X2"));
        return sb.ToString();
    }
}
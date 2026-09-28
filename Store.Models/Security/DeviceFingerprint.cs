using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Store.Models.Security;

/// <summary>
/// SEC-23 — derive a stable device fingerprint from raw request headers.
///
/// Components:
///   - User-Agent (lowercased, trimmed)
///   - IP address normalized to /24 (IPv4) or /48 (IPv6) so mobile-network
///     IP rotation does not register as a new device
///   - Accept-Language (lowercased, trimmed) — second-order signal that
///     catches "same UA + same network but different human" attempts
///
/// Hash is SHA-256 hex (64 chars). Never logged, never sent to the client.
/// </summary>
public static class DeviceFingerprint
{
    /// <summary>Compute the canonical fingerprint hash for a device.</summary>
    public static string ComputeHash(string? userAgent, string? ipAddress, string? acceptLanguage)
    {
        var normalized = string.Join('|',
            NormalizeUserAgent(userAgent),
            NormalizeIp(ipAddress),
            NormalizeAcceptLanguage(acceptLanguage));

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Normalize an IP address to its network prefix.
    /// IPv4 → /24 (drops last octet); IPv6 → /48 (drops last 80 bits).
    /// Null / unparseable → "unknown".
    /// </summary>
    public static string NormalizeIp(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress)) return "unknown";
        if (!IPAddress.TryParse(ipAddress.Trim(), out var ip)) return "unknown";

        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = ip.GetAddressBytes();
            return $"{bytes[0]}.{bytes[1]}.{bytes[2]}.0/24";
        }
        if (ip.AddressFamily == AddressFamily.InterNetworkV6)
        {
            var bytes = ip.GetAddressBytes();
            // /48 keeps the first 6 bytes (48 bits).
            return $"{bytes[0]:x2}{bytes[1]:x2}:{bytes[2]:x2}{bytes[3]:x2}:{bytes[4]:x2}{bytes[5]:x2}::/48";
        }
        return "unknown";
    }

    /// <summary>Best-effort friendly device name from User-Agent.</summary>
    public static string DeriveName(string? userAgent)
    {
        var ua = NormalizeUserAgent(userAgent);
        if (ua == "unknown") return "Unknown device";

        // Mobile-platform tokens first — iOS / Android UAs also embed the
        // browser ("Safari/", "Chrome/") as substrings, so platform wins.
        if (ua.Contains("iphone")) return "iPhone (Safari)";
        if (ua.Contains("ipad")) return "iPad (Safari)";
        if (ua.Contains("android")) return "Android device";

        // Browser identity on desktop — Edge / Chrome / Firefox / Safari
        // (must come before "windows" / "mac os" so a Chrome-on-Windows
        // UA is not misclassified as "Windows").
        if (ua.Contains("edg/")) return "Edge";
        if (ua.Contains("firefox/")) return "Firefox";
        if (ua.Contains("chrome/")) return "Chrome";
        if (ua.Contains("safari/")) return "Safari";

        if (ua.Contains("windows")) return "Windows";
        if (ua.Contains("mac os") || ua.Contains("macintosh")) return "macOS";
        if (ua.Contains("linux")) return "Linux";

        return "Unknown device";
    }

    private static string NormalizeUserAgent(string? ua)
    {
        if (string.IsNullOrWhiteSpace(ua)) return "unknown";
        return ua.Trim().ToLowerInvariant();
    }

    private static string NormalizeAcceptLanguage(string? lang)
    {
        if (string.IsNullOrWhiteSpace(lang)) return "unknown";
        // Take the first language tag only — quality factors vary but the
        // primary tag is enough to differentiate "French-speaking browser"
        // from "English-speaking browser".
        var primary = lang.Split(',', 2)[0].Trim().ToLowerInvariant();
        return string.IsNullOrEmpty(primary) ? "unknown" : primary;
    }
}

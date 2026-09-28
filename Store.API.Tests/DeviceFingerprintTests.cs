using Store.Models.Security;

namespace Store.API.Tests;

/// <summary>
/// SEC-23 — pure-function tests for <see cref="DeviceFingerprint"/>.
/// Covers hashing stability, IP /24 + /48 normalization, Accept-Language
/// trimming, UA → friendly-name derivation, and collision semantics.
/// </summary>
public class DeviceFingerprintTests
{
    // ---- ComputeHash ----

    [Fact]
    public void ComputeHash_returns_64_char_hex()
    {
        var hash = DeviceFingerprint.ComputeHash("Mozilla/5.0", "10.0.0.1", "en-US");
        Assert.Equal(64, hash.Length);
        Assert.Matches("^[0-9a-f]{64}$", hash);
    }

    [Fact]
    public void ComputeHash_is_deterministic_for_same_inputs()
    {
        var ua = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15";
        var ip = "192.168.1.42";
        var lang = "en-US,en;q=0.9";

        var a = DeviceFingerprint.ComputeHash(ua, ip, lang);
        var b = DeviceFingerprint.ComputeHash(ua, ip, lang);

        Assert.Equal(a, b);
    }

    [Fact]
    public void ComputeHash_normalizes_user_agent_case_and_trim()
    {
        var a = DeviceFingerprint.ComputeHash("Mozilla/5.0", "10.0.0.1", "en-US");
        var b = DeviceFingerprint.ComputeHash("  MOZILLA/5.0  ", "10.0.0.1", "en-US");
        Assert.Equal(a, b);
    }

    [Fact]
    public void ComputeHash_treats_null_and_empty_equivalently()
    {
        var a = DeviceFingerprint.ComputeHash(null, null, null);
        var b = DeviceFingerprint.ComputeHash(string.Empty, "  ", "");
        Assert.Equal(a, b);
    }

    [Fact]
    public void ComputeHash_differs_when_user_agent_changes()
    {
        var a = DeviceFingerprint.ComputeHash("Mozilla/5.0", "10.0.0.1", "en-US");
        var b = DeviceFingerprint.ComputeHash("Mozilla/5.1", "10.0.0.1", "en-US");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ComputeHash_differs_when_ip_changes_outside_subnet()
    {
        var a = DeviceFingerprint.ComputeHash("Mozilla/5.0", "10.0.0.1", "en-US");
        var b = DeviceFingerprint.ComputeHash("Mozilla/5.0", "10.0.0.2", "en-US");
        Assert.Equal(a, b); // same /24 → same hash
        var c = DeviceFingerprint.ComputeHash("Mozilla/5.0", "10.0.1.1", "en-US");
        Assert.NotEqual(a, c); // different /24 → different hash
    }

    [Fact]
    public void ComputeHash_differs_when_accept_language_changes()
    {
        var a = DeviceFingerprint.ComputeHash("Mozilla/5.0", "10.0.0.1", "en-US");
        var b = DeviceFingerprint.ComputeHash("Mozilla/5.0", "10.0.0.1", "fr-FR");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ComputeHash_takes_first_language_tag_only()
    {
        // Quality factors shouldn't matter for the primary tag.
        var a = DeviceFingerprint.ComputeHash("ua", "1.2.3.4", "en-US,fr;q=0.5");
        var b = DeviceFingerprint.ComputeHash("ua", "1.2.3.4", "en-US,de;q=0.9");
        Assert.Equal(a, b);
    }

    // ---- NormalizeIp ----

    [Theory]
    [InlineData("10.0.0.1", "10.0.0.0/24")]
    [InlineData("192.168.42.99", "192.168.42.0/24")]
    [InlineData("255.255.255.255", "255.255.255.0/24")]
    public void NormalizeIp_collapses_to_24_for_ipv4(string input, string expected)
        => Assert.Equal(expected, DeviceFingerprint.NormalizeIp(input));

    [Theory]
    [InlineData("2001:db8:1234:5678::1", "2001:0db8:1234::/48")]
    [InlineData("fe80::1", "fe80:0000:0000::/48")]
    public void NormalizeIp_collapses_to_48_for_ipv6(string input, string expected)
        => Assert.Equal(expected, DeviceFingerprint.NormalizeIp(input));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not an ip")]
    public void NormalizeIp_returns_unknown_for_invalid(string? input)
        => Assert.Equal("unknown", DeviceFingerprint.NormalizeIp(input));

    // ---- DeriveName ----

    [Theory]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1", "iPhone (Safari)")]
    [InlineData("Mozilla/5.0 (iPad; CPU OS 17_0 like Mac OS X) AppleWebKit/605.1.15 Version/17.0 Mobile/15E148 Safari/604.1", "iPad (Safari)")]
    [InlineData("Mozilla/5.0 (Linux; Android 14) AppleWebKit/537.36", "Android device")]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64)", "Windows")]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7)", "macOS")]
    [InlineData("Mozilla/5.0 (X11; Linux x86_64)", "Linux")]
    [InlineData("Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36 Edg/118.0.0.0", "Edge")]
    [InlineData("Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/118.0.0.0 Safari/537.36", "Chrome")]
    [InlineData("Mozilla/5.0 (X11; Linux x86_64; rv:120.0) Gecko/20100101 Firefox/120.0", "Firefox")]
    [InlineData("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 Version/17.0 Safari/605.1.15", "Safari")]
    public void DeriveName_returns_friendly_name(string ua, string expected)
        => Assert.Equal(expected, DeviceFingerprint.DeriveName(ua));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("???")]
    public void DeriveName_falls_back_to_unknown_device(string? ua)
        => Assert.Equal("Unknown device", DeviceFingerprint.DeriveName(ua));
}

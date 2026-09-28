using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Store.DbServices.Services;
using Xunit;

namespace Store.API.Tests;

/// <summary>
/// MT-02 — pure-function tests for the PayDunya IPN signature verifier.
/// Network-touching parts (CreateInvoice / ConfirmPayment) are validated
/// via integration tests against the sandbox tenant; here we lock down
/// the security boundary.
/// </summary>
public class PayDunyaPaymentServiceTests
{
    private static PayDunyaPaymentService BuildService(string webhookSecret = "test-secret-32-bytes-long-______")
    {
        var opts = new PayDunyaOptions
        {
            UseSandbox = true,
            MasterKey = "m",
            PrivateKey = "p",
            PublicKey = "pk",
            Token = "t",
            WebhookSecret = webhookSecret
        };
        var http = new HttpClient { BaseAddress = new Uri("https://example.invalid/") };
        return new PayDunyaPaymentService(http, NullLogger<PayDunyaPaymentService>.Instance, Options.Create(opts));
    }

    private static string HmacHex(string secret, string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    [Fact]
    public void VerifyIpn_returns_false_for_missing_secret()
    {
        var svc = BuildService(webhookSecret: "");
        Assert.False(svc.VerifyIpnSignature("{}", "abc"));
    }

    [Fact]
    public void VerifyIpn_returns_false_for_missing_payload()
    {
        var svc = BuildService();
        Assert.False(svc.VerifyIpnSignature("", HmacHex("test-secret", "{}")));
    }

    [Fact]
    public void VerifyIpn_returns_false_for_missing_signature()
    {
        var svc = BuildService();
        Assert.False(svc.VerifyIpnSignature("{}", ""));
    }

    [Fact]
    public void VerifyIpn_accepts_correct_signature()
    {
        const string secret = "test-secret-32-bytes-long-______";
        const string payload = "{\"token\":\"abc\",\"status\":\"completed\"}";
        var signature = HmacHex(secret, payload);
        var svc = BuildService(secret);
        Assert.True(svc.VerifyIpnSignature(payload, signature));
    }

    [Fact]
    public void VerifyIpn_accepts_correct_signature_uppercase_hex()
    {
        const string secret = "test-secret-32-bytes-long-______";
        const string payload = "{\"token\":\"abc\"}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var signature = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))); // uppercase
        var svc = BuildService(secret);
        Assert.True(svc.VerifyIpnSignature(payload, signature));
    }

    [Fact]
    public void VerifyIpn_rejects_tampered_payload()
    {
        const string secret = "test-secret-32-bytes-long-______";
        var signature = HmacHex(secret, "{\"amount\":100}");
        var svc = BuildService(secret);
        Assert.False(svc.VerifyIpnSignature("{\"amount\":999}", signature));
    }

    [Fact]
    public void VerifyIpn_rejects_wrong_secret()
    {
        var signature = HmacHex("other-secret", "{}");
        var svc = BuildService("test-secret-32-bytes-long-______");
        Assert.False(svc.VerifyIpnSignature("{}", signature));
    }

    [Fact]
    public void VerifyIpn_is_case_insensitive_on_signature()
    {
        const string secret = "test-secret-32-bytes-long-______";
        var signature = HmacHex(secret, "{}");
        var svc = BuildService(secret);
        Assert.True(svc.VerifyIpnSignature("{}", signature.ToUpperInvariant()));
    }

    [Fact]
    public void VerifyIpn_rejects_signature_with_garbage_chars()
    {
        var svc = BuildService();
        Assert.False(svc.VerifyIpnSignature("{}", "not-a-hex-string-at-all"));
    }

    [Fact]
    public void VerifyIpn_rejects_signature_of_wrong_length()
    {
        var svc = BuildService();
        Assert.False(svc.VerifyIpnSignature("{}", "abc"));
    }
}
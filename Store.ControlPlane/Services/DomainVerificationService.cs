using System.Security.Cryptography;
using DnsClient;
using Store.ControlPlane.Models.DTOs;

namespace Store.ControlPlane.Services;

public class DomainVerificationService : IDomainVerificationService
{
    private readonly ILookupClient _lookupClient;
    private readonly ILogger<DomainVerificationService> _logger;

    public DomainVerificationService(ILogger<DomainVerificationService> logger)
    {
        _logger = logger;
        // Use reliable public DNS resolvers (Cloudflare 1.1.1.1, Google 8.8.8.8) with short timeout
        _lookupClient = new LookupClient(new LookupClientOptions(
            NameServer.Cloudflare,
            NameServer.GooglePublicDns)
        {
            Timeout = TimeSpan.FromSeconds(5),
            UseCache = false // Live verification requires fresh DNS responses
        });
    }

    public string GenerateVerificationToken()
    {
        var bytes = new byte[24];
        RandomNumberGenerator.Fill(bytes);
        return $"clxv_{Convert.ToHexString(bytes).ToLowerInvariant()}";
    }

    public string GetVerificationHost(string domain)
    {
        var cleanDomain = domain.Trim().ToLowerInvariant().TrimEnd('.');
        return $"_clexan-verify.{cleanDomain}";
    }

    public async Task<VerifyDomainResponse> VerifyTxtRecordAsync(string domain, string expectedToken, CancellationToken ct = default)
    {
        var checkHost = GetVerificationHost(domain);
        var foundValues = new List<string>();

        try
        {
            _logger.LogInformation("Querying DNS TXT records for {Host}", checkHost);
            var result = await _lookupClient.QueryAsync(checkHost, QueryType.TXT, cancellationToken: ct);

            if (result.HasError)
            {
                _logger.LogWarning("DNS query returned error for {Host}: {ErrorMessage}", checkHost, result.ErrorMessage);
                return new VerifyDomainResponse(
                    Domain: domain,
                    IsVerified: false,
                    Status: "Pending",
                    CheckedHost: checkHost,
                    ExpectedValue: expectedToken,
                    FoundValues: foundValues,
                    Message: $"DNS record not found yet ({result.ErrorMessage}). Note: DNS propagation may take a few minutes."
                );
            }

            foreach (var txtRecord in result.Answers.TxtRecords())
            {
                foreach (var val in txtRecord.Text)
                {
                    foundValues.Add(val);
                    if (string.Equals(val.Trim(), expectedToken.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("DNS TXT ownership successfully verified for domain {Domain} on host {Host}", domain, checkHost);
                        return new VerifyDomainResponse(
                            Domain: domain,
                            IsVerified: true,
                            Status: "Verified",
                            CheckedHost: checkHost,
                            ExpectedValue: expectedToken,
                            FoundValues: foundValues,
                            Message: "Domain verified successfully. Traefik reverse-proxy routing updated."
                        );
                    }
                }
            }

            _logger.LogWarning("DNS TXT record found but token did not match for {Host}. Found: {FoundCount} records", checkHost, foundValues.Count);
            return new VerifyDomainResponse(
                Domain: domain,
                IsVerified: false,
                Status: "Pending",
                CheckedHost: checkHost,
                ExpectedValue: expectedToken,
                FoundValues: foundValues,
                Message: foundValues.Count > 0 
                    ? "DNS TXT record found but the value does not match the verification token." 
                    : "No TXT record found at _clexan-verify." + domain
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DNS lookup exception for {Host}", checkHost);
            return new VerifyDomainResponse(
                Domain: domain,
                IsVerified: false,
                Status: "Failed",
                CheckedHost: checkHost,
                ExpectedValue: expectedToken,
                FoundValues: foundValues,
                Message: $"DNS query failed: {ex.Message}"
            );
        }
    }
}

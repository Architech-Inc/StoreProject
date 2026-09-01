using Store.ControlPlane.Models.DTOs;

namespace Store.ControlPlane.Services;

public interface IDomainVerificationService
{
    string GenerateVerificationToken();
    string GetVerificationHost(string domain);
    Task<VerifyDomainResponse> VerifyTxtRecordAsync(string domain, string expectedToken, CancellationToken ct = default);
}

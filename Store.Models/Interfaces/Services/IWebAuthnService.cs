using Fido2NetLib;
using Fido2NetLib.Objects;
using Store.Models.DTOs.Common;

namespace Store.Models.Interfaces.Services;

public interface IWebAuthnService
{
    Task<CredentialCreateOptions> RequestNewCredentialAsync(Guid userId, CancellationToken ct = default);
    Task<Fido2NetLib.Objects.RegisteredPublicKeyCredential> RegisterNewCredentialAsync(Guid userId, AuthenticatorAttestationRawResponse attestationResponse, CancellationToken ct = default);
    
    Task<AssertionOptions> RequestAssertionAsync(string username, CancellationToken ct = default);
    Task<(Fido2NetLib.Objects.VerifyAssertionResult Result, Guid UserId)> MakeAssertionAsync(AuthenticatorAssertionRawResponse assertionResponse, CancellationToken ct = default);

    Task<List<Store.Models.DTOs.Auth.FidoCredentialDto>> GetCredentialsAsync(Guid userId, CancellationToken ct = default);
    Task<bool> RemoveCredentialAsync(Guid userId, int credentialId, CancellationToken ct = default);
}

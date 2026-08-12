using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Store.DbServices.Context;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;
using System.Text;

namespace Store.API.Services;

public class WebAuthnService : IWebAuthnService
{
    private readonly IFido2 _fido2;
    private readonly StoreDbContext _dbContext;
    private readonly IMemoryCache _cache;

    public WebAuthnService(IFido2 fido2, StoreDbContext dbContext, IMemoryCache cache)
    {
        _fido2 = fido2;
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<CredentialCreateOptions> RequestNewCredentialAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.FidoCredentials)
            .FirstOrDefaultAsync(u => u.UserId == userId, ct);

        if (user == null)
            throw new Exception("User not found");

        var existingKeys = user.FidoCredentials
            .Select(c => new PublicKeyCredentialDescriptor(c.CredentialId))
            .ToList();

        var userHandle = Encoding.UTF8.GetBytes(user.UserId.ToString());

        var fidoUser = new Fido2User
        {
            DisplayName = user.Username,
            Name = user.Username,
            Id = userHandle
        };

        var authenticatorSelection = new AuthenticatorSelection
        {
            ResidentKey = ResidentKeyRequirement.Discouraged,
            UserVerification = UserVerificationRequirement.Preferred
        };

        var options = _fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = fidoUser,
            ExcludeCredentials = existingKeys,
            AuthenticatorSelection = authenticatorSelection,
            AttestationPreference = AttestationConveyancePreference.None
        });

        _cache.Set($"fido2:attestation:{userId}", options, TimeSpan.FromMinutes(5));

        return options;
    }

    public async Task<Fido2NetLib.Objects.RegisteredPublicKeyCredential> RegisterNewCredentialAsync(Guid userId, AuthenticatorAttestationRawResponse attestationResponse, CancellationToken ct = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId, ct);
        if (user == null) throw new Exception("User not found");

        if (!_cache.TryGetValue($"fido2:attestation:{userId}", out CredentialCreateOptions? options) || options == null)
        {
            throw new Exception("Registration options not found or expired. Please try again.");
        }

        IsCredentialIdUniqueToUserAsyncDelegate callback = async (args, cToken) => 
        {
            var exists = await _dbContext.FidoCredentials.AnyAsync(c => c.CredentialId == args.CredentialId, cToken);
            return !exists;
        };

        var success = await _fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
        {
            AttestationResponse = attestationResponse,
            OriginalOptions = options,
            IsCredentialIdUniqueToUserCallback = callback
        }, ct);

        var newCred = new FidoCredential
        {
            UserId = userId,
            CredentialId = success.Id,
            PublicKey = success.PublicKey,
            UserHandle = success.User.Id,
            SignatureCounter = success.SignCount,
            AaGuid = success.AaGuid,
            CredType = "public-key", // Assuming default string here as v4 might have changed it
            RegDate = DateTime.UtcNow
        };

        await _dbContext.FidoCredentials.AddAsync(newCred, ct);
        await _dbContext.SaveChangesAsync(ct);

        _cache.Remove($"fido2:attestation:{userId}");

        return success;
    }

    public async Task<AssertionOptions> RequestAssertionAsync(string username, CancellationToken ct = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.FidoCredentials)
            .FirstOrDefaultAsync(u => u.Username == username, ct);

        if (user == null || !user.FidoCredentials.Any())
            throw new Exception("User not found or no biometrics registered");

        var existingKeys = user.FidoCredentials
            .Select(c => new PublicKeyCredentialDescriptor(c.CredentialId))
            .ToList();

        var options = _fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = existingKeys,
            UserVerification = UserVerificationRequirement.Preferred
        });

        _cache.Set($"fido2:assertion:{username}", options, TimeSpan.FromMinutes(5));

        return options;
    }

    public async Task<(Fido2NetLib.Objects.VerifyAssertionResult Result, Guid UserId)> MakeAssertionAsync(AuthenticatorAssertionRawResponse assertionResponse, CancellationToken ct = default)
    {
        var fidoCred = await _dbContext.FidoCredentials
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.CredentialId == assertionResponse.RawId, ct);

        if (fidoCred == null)
            throw new Exception("Credential not found");

        var username = fidoCred.User.Username;

        if (!_cache.TryGetValue($"fido2:assertion:{username}", out AssertionOptions? options) || options == null)
        {
            throw new Exception("Assertion options not found or expired. Please try again.");
        }

        IsUserHandleOwnerOfCredentialIdAsync callback = async (args, cToken) => 
        {
            return fidoCred.UserHandle.SequenceEqual(args.UserHandle);
        };

        var res = await _fido2.MakeAssertionAsync(new MakeAssertionParams
        {
            AssertionResponse = assertionResponse,
            OriginalOptions = options,
            StoredPublicKey = fidoCred.PublicKey,
            StoredSignatureCounter = fidoCred.SignatureCounter,
            IsUserHandleOwnerOfCredentialIdCallback = callback
        }, ct);

        fidoCred.SignatureCounter = res.SignCount;
        _dbContext.FidoCredentials.Update(fidoCred);
        await _dbContext.SaveChangesAsync(ct);

        _cache.Remove($"fido2:assertion:{username}");

        return (res, fidoCred.UserId);
    }

    public async Task<List<Store.Models.DTOs.Auth.FidoCredentialDto>> GetCredentialsAsync(Guid userId, CancellationToken ct = default)
    {
        var credentials = await _dbContext.FidoCredentials
            .Where(c => c.UserId == userId)
            .Select(c => new Store.Models.DTOs.Auth.FidoCredentialDto
            {
                Id = c.Id,
                CredentialType = c.CredType,
                RegistrationDate = c.RegDate,
                AaGuid = c.AaGuid.ToString()
            })
            .ToListAsync(ct);

        return credentials;
    }

    public async Task<bool> RemoveCredentialAsync(Guid userId, int credentialId, CancellationToken ct = default)
    {
        var credential = await _dbContext.FidoCredentials
            .FirstOrDefaultAsync(c => c.Id == credentialId && c.UserId == userId, ct);

        if (credential == null) return false;

        _dbContext.FidoCredentials.Remove(credential);
        await _dbContext.SaveChangesAsync(ct);

        return true;
    }
}

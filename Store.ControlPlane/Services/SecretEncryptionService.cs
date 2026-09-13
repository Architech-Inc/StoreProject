using System.Security.Cryptography;
using System.Text;

namespace Store.ControlPlane.Services;

/// <summary>
/// AES-GCM envelope encryption for tenant secrets at rest.
/// The master key MUST be supplied via configuration (env var `ControlPlane__MasterEncryptionKey`
/// or user-secrets). The service refuses to start when it is missing.
/// </summary>
public class SecretEncryptionService : ISecretEncryptionService
{
    private readonly byte[] _key;
    private const string EncryptedPrefix = "enc:v1:";

    public SecretEncryptionService(IConfiguration config)
    {
        var rawKey = config["ControlPlane:MasterEncryptionKey"]
            ?? throw new InvalidOperationException(
                "ControlPlane:MasterEncryptionKey is required. " +
                "Set it via env var ControlPlane__MasterEncryptionKey or user-secrets. " +
                "Refusing to start with a default/fallback key.");

        if (rawKey.Length < 32)
        {
            throw new InvalidOperationException(
                "ControlPlane:MasterEncryptionKey must be at least 32 characters. " +
                "Generate with: openssl rand -base64 48");
        }

        // Reject well-known placeholder values shipped in earlier revisions
        if (rawKey.Contains("REPLACE_WITH", StringComparison.OrdinalIgnoreCase) ||
            rawKey.Contains("MasterSecretKey2026", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "ControlPlane:MasterEncryptionKey is still set to a known placeholder value. " +
                "Generate a new secret and rotate any previously encrypted values.");
        }

        using var sha = SHA256.Create();
        _key = sha.ComputeHash(Encoding.UTF8.GetBytes(rawKey));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;
        if (plainText.StartsWith(EncryptedPrefix, StringComparison.Ordinal)) return plainText;

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];     // 16 bytes
        var cipherBytes = new byte[plainBytes.Length];

        RandomNumberGenerator.Fill(nonce);

        using var aesGcm = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

        // Combined: Nonce (12) + Tag (16) + CipherText
        var combined = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, combined, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, combined, nonce.Length + tag.Length, cipherBytes.Length);

        return EncryptedPrefix + Convert.ToBase64String(combined);
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return cipherText;
        if (!cipherText.StartsWith(EncryptedPrefix, StringComparison.Ordinal))
        {
            // Already plain or not encrypted
            return cipherText;
        }

        try
        {
            var base64 = cipherText[EncryptedPrefix.Length..];
            var combined = Convert.FromBase64String(base64);

            var nonceSize = AesGcm.NonceByteSizes.MaxSize;
            var tagSize = AesGcm.TagByteSizes.MaxSize;

            if (combined.Length < nonceSize + tagSize)
            {
                return cipherText;
            }

            var nonce = new byte[nonceSize];
            var tag = new byte[tagSize];
            var cipherBytes = new byte[combined.Length - nonceSize - tagSize];

            Buffer.BlockCopy(combined, 0, nonce, 0, nonceSize);
            Buffer.BlockCopy(combined, nonceSize, tag, 0, tagSize);
            Buffer.BlockCopy(combined, nonceSize + tagSize, cipherBytes, 0, cipherBytes.Length);

            var plainBytes = new byte[cipherBytes.Length];
            using var aesGcm = new AesGcm(_key, tagSize);
            aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            // If decryption fails, return as-is to avoid hard crash
            return cipherText;
        }
    }
}

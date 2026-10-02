using System;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

/// <summary>
/// SEC-24 — Production-grade password hasher utilizing Argon2id.
/// Adheres to OWASP Password Storage guidelines (19 MiB memory, 2 iterations, 1 lane).
/// Seamlessly verifies legacy BCrypt hashes and signals re-hash requirement for transparent migration.
/// </summary>
public class Argon2idPasswordHasher : IPasswordHasher
{
    public const int DefaultMemorySizeKb = 19456; // 19 MiB (OWASP recommended minimum)
    public const int DefaultIterations = 2;
    public const int DefaultParallelism = 1;
    public const int DefaultSaltSize = 16;       // 128-bit cryptographically secure salt
    public const int DefaultHashSize = 32;       // 256-bit derived key

    private readonly int _memorySizeKb;
    private readonly int _iterations;
    private readonly int _parallelism;
    private readonly int _saltSize;
    private readonly int _hashSize;

    /// <summary>Default singleton instance for use in non-DI contexts (seeding, migration scripts, tenant orchestration).</summary>
    public static readonly Argon2idPasswordHasher Default = new();

    public Argon2idPasswordHasher(
        int memorySizeKb = DefaultMemorySizeKb,
        int iterations = DefaultIterations,
        int parallelism = DefaultParallelism,
        int saltSize = DefaultSaltSize,
        int hashSize = DefaultHashSize)
    {
        _memorySizeKb = memorySizeKb;
        _iterations = iterations;
        _parallelism = parallelism;
        _saltSize = saltSize;
        _hashSize = hashSize;
    }

    /// <summary>Static helper to hash password using default OWASP Argon2id parameters.</summary>
    public static string Hash(string password) => Default.HashPassword(password);

    /// <summary>Static helper to verify password against stored Argon2id or legacy BCrypt hash.</summary>
    public static bool Verify(string password, string storedHash) => Default.VerifyPassword(password, storedHash);

    public string HashPassword(string password)
    {
        if (password is null)
            throw new ArgumentNullException(nameof(password));

        var salt = RandomNumberGenerator.GetBytes(_saltSize);

        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = _memorySizeKb,
            Iterations = _iterations,
            DegreeOfParallelism = _parallelism
        };

        var hashBytes = argon2.GetBytes(_hashSize);
        var saltBase64 = Convert.ToBase64String(salt);
        var hashBase64 = Convert.ToBase64String(hashBytes);

        // Standard modular crypt format: $argon2id$v=19$m=19456,t=2,p=1$<salt>$<hash>
        return $"$argon2id$v=19$m={_memorySizeKb},t={_iterations},p={_parallelism}${saltBase64}${hashBase64}";
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            return false;

        // Check for Argon2id hash format
        if (storedHash.StartsWith("$argon2id$", StringComparison.OrdinalIgnoreCase))
        {
            return VerifyArgon2id(password, storedHash);
        }

        // Backward compatibility: Legacy BCrypt hash ($2a$, $2b$, or $2y$)
        if (storedHash.StartsWith("$2a$", StringComparison.Ordinal) ||
            storedHash.StartsWith("$2b$", StringComparison.Ordinal) ||
            storedHash.StartsWith("$2y$", StringComparison.Ordinal))
        {
            try
            {
                return BCrypt.Net.BCrypt.EnhancedVerify(password, storedHash) ||
                       BCrypt.Net.BCrypt.Verify(password, storedHash);
            }
            catch
            {
                return false;
            }
        }

        return false;
    }

    public bool NeedsRehash(string storedHash)
    {
        if (string.IsNullOrWhiteSpace(storedHash))
            return true;

        // If not already Argon2id, it must be upgraded
        if (!storedHash.StartsWith("$argon2id$", StringComparison.OrdinalIgnoreCase))
            return true;

        // If Argon2id, verify parameters match current configuration
        try
        {
            var parts = storedHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5) return true;

            var paramPairs = parts[2].Split(',');
            int m = 0, t = 0, p = 0;
            foreach (var pair in paramPairs)
            {
                var kv = pair.Split('=');
                if (kv.Length == 2)
                {
                    if (kv[0] == "m") int.TryParse(kv[1], out m);
                    else if (kv[0] == "t") int.TryParse(kv[1], out t);
                    else if (kv[0] == "p") int.TryParse(kv[1], out p);
                }
            }

            return m < _memorySizeKb || t < _iterations || p < _parallelism;
        }
        catch
        {
            return true;
        }
    }

    private static bool VerifyArgon2id(string password, string storedHash)
    {
        try
        {
            var parts = storedHash.Split('$', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5 || !parts[0].Equals("argon2id", StringComparison.OrdinalIgnoreCase))
                return false;

            int memorySize = DefaultMemorySizeKb;
            int iterations = DefaultIterations;
            int parallelism = DefaultParallelism;

            var paramPairs = parts[2].Split(',');
            foreach (var pair in paramPairs)
            {
                var kv = pair.Split('=');
                if (kv.Length == 2)
                {
                    if (kv[0] == "m" && int.TryParse(kv[1], out var m)) memorySize = m;
                    else if (kv[0] == "t" && int.TryParse(kv[1], out var t)) iterations = t;
                    else if (kv[0] == "p" && int.TryParse(kv[1], out var p)) parallelism = p;
                }
            }

            var salt = Convert.FromBase64String(parts[3]);
            var expectedHash = Convert.FromBase64String(parts[4]);

            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                MemorySize = memorySize,
                Iterations = iterations,
                DegreeOfParallelism = parallelism
            };

            var computedHash = argon2.GetBytes(expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(computedHash, expectedHash);
        }
        catch
        {
            return false;
        }
    }
}

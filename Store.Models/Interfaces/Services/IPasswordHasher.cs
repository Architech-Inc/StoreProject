namespace Store.Models.Interfaces.Services;

/// <summary>
/// Defines password hashing, verification, and upgrade-evaluation operations.
/// Supports state-of-the-art Argon2id hashing with backward-compatible verification
/// and seamless runtime migration from legacy BCrypt hashes.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plaintext password using Argon2id with OWASP-recommended parameters.
    /// </summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>A modular crypt-formatted Argon2id hash string.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies whether the provided plaintext password matches the stored hash
    /// (supports both modern Argon2id and legacy BCrypt formats).
    /// </summary>
    /// <param name="password">The plaintext password candidate.</param>
    /// <param name="storedHash">The stored hash string.</param>
    /// <returns><c>true</c> if the password matches the hash; otherwise <c>false</c>.</returns>
    bool VerifyPassword(string password, string storedHash);

    /// <summary>
    /// Determines whether the stored hash should be upgraded to modern Argon2id
    /// (returns <c>true</c> if the hash is still stored in legacy BCrypt format).
    /// </summary>
    /// <param name="storedHash">The stored hash string.</param>
    /// <returns><c>true</c> if the password record should be re-hashed; otherwise <c>false</c>.</returns>
    bool NeedsRehash(string storedHash);
}

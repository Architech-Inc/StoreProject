namespace Store.Models.Interfaces.Services;

/// <summary>
/// SEC-27 — k-anonymity password breach check.
///
/// Implementations MUST hash the password, send only the first 5 hex chars
/// of the SHA-1 hash to a remote service, and look up the remaining 35 chars
/// in the returned list. This way the full password never leaves the
/// server — see <see href="https://haveibeenpwned.com/API/v3#PwnedPasswords"/>.
/// </summary>
public interface IBreachChecker
{
    /// <summary>
    /// Returns true if the password has been seen in any public breach.
    /// Returns false on network error (fail open) — callers should still
    /// enforce the rest of their password policy.
    /// </summary>
    Task<bool> IsBreachedAsync(string password, CancellationToken ct = default);
}
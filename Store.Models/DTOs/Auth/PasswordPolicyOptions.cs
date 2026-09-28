using System.ComponentModel.DataAnnotations;

namespace Store.Models.DTOs.Auth;

/// <summary>
/// SEC-26 — server-side password complexity policy.
///
/// Loaded from <c>Auth:PasswordPolicy</c> config section at startup and
/// injected into <see cref="PasswordComplexityAttribute"/>. Defaults are
/// deliberately conservative (NIST SP 800-63B inspired):
///   - minimum 12 characters (length is the single biggest lever)
///   - at least 3 of the 4 character classes (upper / lower / digit / symbol)
///   - reject a small embedded blocklist of universally-terrible passwords
///   - optional HIBP k-anonymity breach check (off by default; turn on in
///     production via config once outbound network is allowed).
/// </summary>
public class PasswordPolicyOptions
{
    public const string SectionName = "Auth:PasswordPolicy";

    public int MinLength { get; set; } = 12;
    public int MaxLength { get; set; } = 128;
    public int MinClassCount { get; set; } = 3;

    /// <summary>
    /// When true, <see cref="PasswordComplexityAttribute"/> consults
    /// <see cref="Store.Models.Interfaces.Services.IBreachChecker"/> before
    /// accepting a password. Defaults to false — production should turn this
    /// on once the HIBP endpoint is reachable from the deployment network.
    /// </summary>
    public bool EnableBreachCheck { get; set; } = false;

    /// <summary>
    /// Optional extra blocklist applied locally (always on, no network).
    /// Combine with HIBP for full coverage. Stored as a hash set at startup.
    /// </summary>
    public List<string> CommonPasswords { get; set; } = new();

    public static PasswordPolicyOptions WithSecureDefaults() => new()
    {
        MinLength = 12,
        MaxLength = 128,
        MinClassCount = 3,
        EnableBreachCheck = false,
        CommonPasswords = DefaultCommonPasswords.ToList()
    };

    /// <summary>
    /// Top-100 universally-terrible passwords from public breach corpuses.
    /// This is intentionally tiny — HIBP catches the long tail; this list
    /// is just for the worst offenders so we don't have to make a network
    /// call to reject "password123".
    /// </summary>
    public static readonly string[] DefaultCommonPasswords =
    {
        "password", "password1", "password123", "password1234", "p@ssw0rd",
        "p@ssword", "12345678", "123456789", "1234567890", "qwerty",
        "qwerty123", "qwertyuiop", "iloveyou", "letmein", "admin",
        "admin123", "administrator", "welcome", "welcome1", "welcome123",
        "monkey", "monkey123", "dragon", "dragon123", "master",
        "master123", "football", "baseball", "shadow", "shadow123",
        "sunshine", "sunshine1", "princess", "trustno1", "abc123",
        "abc1234", "111111", "000000", "654321", "1234567",
        "superman", "batman", "starwars", "hello123", "freedom",
        "whatever", "qazwsx", "trustme", "jordan23", "harley",
        "zxcvbnm", "asdfghjkl", "computer", "internet", "soccer",
        "hockey", "killer", "george", "andrew", "matrix",
        "jessica", "jennifer", "michelle", "amanda", "ashley",
        "nicole", "sandra", "maggie", "cookie", "killer1",
        "thomas", "robert", "jordan", "michael1", "summer",
        "charlie", "donald", "mustang", "bailey", "buster",
        "sophie", "harold", "ginger", "hannah", "yellow",
        "purple", "dallas", "austin", "thunder", "taylor"
    };
}

/// <summary>
/// SEC-26 — ValidationAttribute that runs the
/// <see cref="PasswordPolicyOptions"/> checks on a password property.
///
/// Requires DI: register <see cref="PasswordPolicyHolder"/> as a singleton
/// during startup so the attribute can pull the live config. The attribute
/// uses <c>IServiceProvider</c> from <see cref="ValidationContext"/> to
/// resolve dependencies (see <c>ValidationContext.GetService</c>).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class PasswordComplexityAttribute : ValidationAttribute
{
    public PasswordComplexityAttribute()
        : base("Password does not meet complexity requirements.")
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var password = value as string;
        if (string.IsNullOrEmpty(password))
        {
            // [Required] already covers this; don't double-fail.
            return ValidationResult.Success;
        }

        var holder = validationContext.GetService(typeof(PasswordPolicyHolder)) as PasswordPolicyHolder;
        var policy = holder?.Policy;

        if (policy is null)
        {
            // No policy bound — accept anything. We don't want to break the
            // pipeline just because config wasn't loaded; callers can still
            // rely on [StringLength] annotations for the basics.
            return ValidationResult.Success;
        }

        if (password.Length < policy.MinLength)
        {
            return new ValidationResult(
                $"Password must be at least {policy.MinLength} characters long.",
                new[] { validationContext.MemberName ?? string.Empty });
        }

        if (password.Length > policy.MaxLength)
        {
            return new ValidationResult(
                $"Password must be at most {policy.MaxLength} characters long.",
                new[] { validationContext.MemberName ?? string.Empty });
        }

        var classes = 0;
        if (password.Any(char.IsUpper)) classes++;
        if (password.Any(char.IsLower)) classes++;
        if (password.Any(char.IsDigit)) classes++;
        if (password.Any(c => !char.IsLetterOrDigit(c))) classes++;

        if (classes < policy.MinClassCount)
        {
            return new ValidationResult(
                $"Password must contain at least {policy.MinClassCount} of: uppercase, lowercase, digit, symbol.",
                new[] { validationContext.MemberName ?? string.Empty });
        }

        // Cheap local blocklist check before any network call.
        if (policy.CommonPasswords.Count > 0 &&
            policy.CommonPasswords.Contains(password, StringComparer.OrdinalIgnoreCase))
        {
            return new ValidationResult(
                "Password appears on a known-bad list. Choose a different password.",
                new[] { validationContext.MemberName ?? string.Empty });
        }

        // Optional HIBP breach check — runs synchronously here for simplicity.
        // The checker itself is responsible for timeouts so the validation
        // pipeline doesn't block on a slow HIBP endpoint.
        if (policy.EnableBreachCheck)
        {
            var breachChecker = validationContext.GetService(typeof(Store.Models.Interfaces.Services.IBreachChecker))
                as Store.Models.Interfaces.Services.IBreachChecker;

            if (breachChecker is not null)
            {
                try
                {
                    if (breachChecker.IsBreachedAsync(password, CancellationToken.None)
                        .GetAwaiter().GetResult())
                    {
                        return new ValidationResult(
                            "Password has appeared in a public data breach. Choose a different password.",
                            new[] { validationContext.MemberName ?? string.Empty });
                    }
                }
                catch
                {
                    // Don't fail validation if HIBP is unreachable — fail open
                    // for breach checks (the rest of the policy still applies).
                }
            }
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Singleton holder so the attribute can pull the live config via
/// <c>ValidationContext.GetService</c> without taking a constructor
/// dependency. Registered once at startup.
/// </summary>
public sealed class PasswordPolicyHolder
{
    public PasswordPolicyOptions Policy { get; }

    public PasswordPolicyHolder(PasswordPolicyOptions policy)
    {
        Policy = policy;
    }
}
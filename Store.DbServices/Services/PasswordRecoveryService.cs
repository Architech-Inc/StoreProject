using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Store.Models.DTOs.Auth;
using Store.Models.Entities;
using Store.Models.Interfaces.Repositories;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;
using System.Security.Cryptography;
using System.Text;

namespace Store.DbServices.Services;

public class PasswordRecoveryService : IPasswordRecoveryService
{
    private const int OtpLength = 6;
    private const int OtpMinValue = 100_000;
    private const int OtpMaxValue = 1_000_000;

    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;
    private readonly ILogger<PasswordRecoveryService> _logger;
    private readonly byte[] _otpPepper;

    public PasswordRecoveryService(
        IUnitOfWork uow,
        INotificationService notificationService,
        ILogger<PasswordRecoveryService> logger,
        OtpPepperOptions pepperOptions)
    {
        _uow = uow;
        _notificationService = notificationService;
        _logger = logger;
        _otpPepper = pepperOptions.GetPepperBytes();

        if (_otpPepper.Length < 32)
        {
            throw new InvalidOperationException(
                "Auth:OtpPepper must be at least 32 bytes (256 bits) to key HMAC-SHA256 for OTP hashing. " +
                "Set the env var Auth__OtpPepper in production.");
        }
    }

    /// <summary>
    /// SEC-06 — Hash the raw OTP code with HMAC-SHA256 keyed by the per-deploy pepper.
    /// Never persists the plaintext code to disk. The returned base64 string is what
    /// we compare against at verify time, via <see cref="CryptographicOperations.FixedTimeEquals"/>.
    /// </summary>
    private string HashOtpCode(string rawCode)
    {
        var codeBytes = Encoding.UTF8.GetBytes(rawCode);
        var hash = HMACSHA256.HashData(_otpPepper, codeBytes);
        return Convert.ToBase64String(hash);
    }

    public async Task<bool> RequestOtpAsync(string username, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>().Query()
            .FirstOrDefaultAsync(u => u.Username == username.Trim(), ct);

        if (user == null) return false;

        // Generate 6 digit OTP using a cryptographic RNG
        var otpCode = RandomNumberGenerator.GetInt32(OtpMinValue, OtpMaxValue).ToString("D" + OtpLength);
        var otp = new Otp
        {
            UserId = user.UserId,
            CodeHash = HashOtpCode(otpCode),
            Purpose = Store.Models.Enums.OtpPurpose.PasswordReset,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false,
            DateCreated = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        await _uow.Repository<Otp>().AddAsync(otp, ct);
        await _uow.SaveChangesAsync(ct);

        // Fetch user's primary contact details to send OTP
        var userWithContacts = await _uow.Repository<User>().Query()
            .Include(u => u.Emails).ThenInclude(e => e.Email)
            .Include(u => u.Phones).ThenInclude(p => p.Phone)
            .FirstOrDefaultAsync(u => u.UserId == user.UserId, ct);

        var primaryEmail = userWithContacts?.Emails?.FirstOrDefault(e => e.IsPrimary)?.Email?.Address
                           ?? userWithContacts?.Emails?.FirstOrDefault()?.Email?.Address;

        var primaryPhone = userWithContacts?.Phones?.FirstOrDefault(p => p.IsPrimary)?.Phone?.Number
                           ?? userWithContacts?.Phones?.FirstOrDefault()?.Phone?.Number;

        if (!string.IsNullOrWhiteSpace(primaryEmail))
        {
            await _notificationService.SendEmailAsync(primaryEmail, "Password Recovery OTP", $"Your OTP is: {otpCode}", user.UserId, ct);
        }
        else if (!string.IsNullOrWhiteSpace(primaryPhone))
        {
            await _notificationService.SendSmsAsync(primaryPhone, $"Your Store password recovery OTP is: {otpCode}", user.UserId, ct);
        }

        return true;
    }

    public async Task<string?> VerifyOtpAsync(string username, string otpCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(otpCode) || otpCode.Length != OtpLength)
        {
            // Bad shape — same wall-clock as if the user didn't exist.
            await Task.Delay(60, ct);
            return null;
        }

        var user = await _uow.Repository<User>().Query()
            .FirstOrDefaultAsync(u => u.Username == username.Trim(), ct);

        if (user == null)
        {
            // Same wall-clock as a real verify path so this endpoint doesn't
            // double as a username oracle beyond the rate-limit on the controller.
            await Task.Delay(60, ct);
            return null;
        }

        var now = DateTime.UtcNow;

        // Load all active OTPs for this user/purpose and compare with constant-time.
        // Loading all matching rows is acceptable here because Otp has
        // (UserId, Purpose, IsUsed, ExpiresAt) indexed; the active set is small (1–2 rows).
        var candidates = await _uow.Repository<Otp>().Query()
            .Where(o => o.UserId == user.UserId
                && o.Purpose == Store.Models.Enums.OtpPurpose.PasswordReset
                && !o.IsUsed
                && o.ExpiresAt > now)
            .ToListAsync(ct);

        var submittedHashBytes = Encoding.UTF8.GetBytes(HashOtpCode(otpCode));

        Otp? match = null;
        foreach (var candidate in candidates)
        {
            // Constant-time equality over the stored HMAC digest.
            var stored = Convert.FromBase64String(candidate.CodeHash);
            if (stored.Length == submittedHashBytes.Length &&
                CryptographicOperations.FixedTimeEquals(stored, submittedHashBytes))
            {
                match = candidate;
                break;
            }
        }

        if (match == null) return null;

        // Mark OTP as used
        match.IsUsed = true;
        match.LastModified = DateTime.UtcNow;
        _uow.Repository<Otp>().Update(match);

        // Issue a PasswordResetToken (separate from the OTP code; only one is
        // accepted at the /api/auth/password-recovery/reset endpoint).
        // .NET 8: use Convert.ToHexString(RandomNumberGenerator.GetBytes(32)) — 64-char hex.
        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var tokenHash = HashToken(rawToken);

        var resetToken = new PasswordResetToken
        {
            UserId = user.UserId,
            TokenHash = tokenHash,
            ExpiryDate = DateTime.UtcNow.AddMinutes(30),
            IsUsed = false,
            DateCreated = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        await _uow.Repository<PasswordResetToken>().AddAsync(resetToken, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Password-recovery OTP verified for user {UserId}; reset token issued, expires {Expiry:o}.",
            user.UserId, resetToken.ExpiryDate);

        return rawToken;
    }

    public async Task<string> IssueTempPasswordAsync(Guid userId, CancellationToken ct = default)
    {
        var userPwd = await _uow.Repository<UserPassword>().Query()
            .FirstOrDefaultAsync(up => up.UserId == userId, ct);

        if (userPwd == null) throw new InvalidOperationException("User password record not found.");

        // SEC-25 — Generate temporary password with cryptographic RNG (was Guid before).
        var rawTempPwd = RandomNumberGenerator.GetString(
            "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789",
            12);

        userPwd.PasswordHash = BCrypt.Net.BCrypt.HashPassword(rawTempPwd);
        userPwd.ForcePasswordChange = true;
        userPwd.TempPasswordExpiresAt = DateTime.UtcNow.AddHours(24);
        userPwd.LastModified = DateTime.UtcNow;

        _uow.Repository<UserPassword>().Update(userPwd);
        await _uow.SaveChangesAsync(ct);

        return rawTempPwd;
    }

    public async Task<bool> ResetPasswordWithTokenAsync(RecoverPasswordWithTokenRequest request, CancellationToken ct = default)
    {
        var tokenHash = HashToken(request.Token);

        var resetToken = await _uow.Repository<PasswordResetToken>().Query()
            .Include(t => t.User)
            .ThenInclude(u => u.Password)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash
                && !t.IsUsed
                && t.ExpiryDate > DateTime.UtcNow, ct);

        if (resetToken == null || resetToken.User?.Password == null) return false;

        // Apply new password
        resetToken.User.Password.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        resetToken.User.Password.ForcePasswordChange = false;
        resetToken.User.Password.TempPasswordExpiresAt = null;
        resetToken.User.Password.LastModified = DateTime.UtcNow;

        // Mark token as used
        resetToken.IsUsed = true;
        resetToken.LastModified = DateTime.UtcNow;

        _uow.Repository<UserPassword>().Update(resetToken.User.Password);
        _uow.Repository<PasswordResetToken>().Update(resetToken);
        await _uow.SaveChangesAsync(ct);

        return true;
    }

    private static string HashToken(string rawToken)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}

/// <summary>
/// Holder for the OTP pepper bytes. The pepper is sourced from <c>Auth:OtpPepper</c>
/// config (env var <c>Auth__OtpPepper</c>). It must be at least 32 bytes (256 bits)
/// to safely key HMAC-SHA256 and is never persisted or logged.
/// </summary>
public sealed class OtpPepperOptions
{
    public const string SectionName = "Auth";

    public string OtpPepper { get; set; } = string.Empty;

    public byte[] GetPepperBytes()
    {
        if (string.IsNullOrWhiteSpace(OtpPepper))
        {
            throw new InvalidOperationException(
                "Auth:OtpPepper is not configured. Set the env var Auth__OtpPepper to a " +
                ">=32-byte secret in production. See Store.DbServices.Services.OtpPepperOptions.");
        }

        // Allow either base64 or raw bytes. If the value parses as base64 AND has at least 32 bytes of entropy,
        // prefer base64; otherwise treat as UTF-8 bytes.
        Span<byte> buffer = stackalloc byte[OtpPepper.Length];
        if (Convert.TryFromBase64String(OtpPepper, buffer, out var base64Len) && base64Len >= 32)
        {
            return buffer[..base64Len].ToArray();
        }

        var raw = Encoding.UTF8.GetBytes(OtpPepper);
        if (raw.Length < 32)
        {
            throw new InvalidOperationException(
                $"Auth:OtpPepper must yield at least 32 bytes when decoded; got {raw.Length} from UTF-8 / {base64Len} from base64.");
        }
        return raw;
    }
}

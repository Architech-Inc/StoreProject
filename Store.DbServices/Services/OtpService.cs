using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;
using System.Security.Cryptography;
using System.Text;

namespace Store.DbServices.Services;

public class OtpService : IOtpService
{
    private readonly IUnitOfWork _uow;
    private readonly byte[] _otpPepper;

    public OtpService(IUnitOfWork uow, IOptions<OtpPepperOptions> pepperOptions)
    {
        _uow = uow;
        _otpPepper = (pepperOptions?.Value ?? throw new ArgumentNullException(nameof(pepperOptions))).GetPepperBytes();
    }

    /// <summary>
    /// SEC-06 — HMAC-SHA256 hash of the OTP code, keyed by the deploy pepper.
    /// Plaintext never touches the DB.
    /// </summary>
    private byte[] HashOtpBytes(string rawCode)
    {
        var codeBytes = Encoding.UTF8.GetBytes(rawCode);
        return HMACSHA256.HashData(_otpPepper, codeBytes);
    }

    private string HashOtpCode(string rawCode)
    {
        return Convert.ToBase64String(HashOtpBytes(rawCode));
    }

    public async Task<string> GenerateAsync(Guid userId, OtpPurpose purpose, CancellationToken ct = default)
    {
        // Invalidate existing OTPs for this user and purpose
        var existing = await _uow.Repository<Otp>().Query()
            .Where(o => o.UserId == userId && o.Purpose == purpose && !o.IsUsed)
            .ToListAsync(ct);

        foreach (var o in existing) { o.IsUsed = true; _uow.Repository<Otp>().Update(o); }

        // SEC-06 — Cryptographic RNG instead of Random.Shared.
        var code = RandomNumberGenerator.GetInt32(100000, 1_000_000).ToString("D6");
        var otp = new Otp
        {
            UserId = userId,
            CodeHash = HashOtpCode(code),
            Purpose = purpose,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false
        };

        await _uow.Repository<Otp>().AddAsync(otp, ct);
        await _uow.SaveChangesAsync(ct);
        return code;
    }

    public async Task<bool> ValidateAsync(Guid userId, string code, OtpPurpose purpose, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 6) return false;

        var candidates = await _uow.Repository<Otp>().Query()
            .Where(o => o.UserId == userId && o.Purpose == purpose && !o.IsUsed)
            .OrderByDescending(o => o.DateCreated)
            .Take(8) // cap so the constant-time walk is bounded
            .ToListAsync(ct);

        var submittedHash = HashOtpBytes(code);

        Otp? match = null;
        foreach (var otp in candidates)
        {
            if (otp.ExpiresAt < DateTime.UtcNow) continue;
            var stored = Convert.FromBase64String(otp.CodeHash);
            if (stored.Length == submittedHash.Length &&
                CryptographicOperations.FixedTimeEquals(stored, submittedHash))
            {
                match = otp;
                break;
            }
        }

        if (match == null) return false;

        match.IsUsed = true;
        _uow.Repository<Otp>().Update(match);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}

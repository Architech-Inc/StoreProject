using Microsoft.EntityFrameworkCore;
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
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;

    public PasswordRecoveryService(IUnitOfWork uow, INotificationService notificationService)
    {
        _uow = uow;
        _notificationService = notificationService;
    }

    public async Task<bool> RequestOtpAsync(string username, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>().Query()
            .FirstOrDefaultAsync(u => u.Username == username.Trim(), ct);

        if (user == null) return false;

        // Generate 6 digit OTP
        var otpCode = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        var otp = new Otp
        {
            UserId = user.UserId,
            Code = otpCode, // Depending on security, you could hash this, but Otp is typically stored as is or encrypted, here assuming plain for typical cases
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
        var user = await _uow.Repository<User>().Query()
            .FirstOrDefaultAsync(u => u.Username == username.Trim(), ct);

        if (user == null) return null;

        var otp = await _uow.Repository<Otp>().Query()
            .FirstOrDefaultAsync(o => o.UserId == user.UserId
                && o.Code == otpCode
                && o.Purpose == Store.Models.Enums.OtpPurpose.PasswordReset
                && !o.IsUsed
                && o.ExpiresAt > DateTime.UtcNow, ct);

        if (otp == null) return null;

        // Mark OTP as used
        otp.IsUsed = true;
        otp.LastModified = DateTime.UtcNow;
        _uow.Repository<Otp>().Update(otp);

        // Issue a PasswordResetToken
        var rawToken = Guid.NewGuid().ToString("N");
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

        return rawToken;
    }

    public async Task<string> IssueTempPasswordAsync(Guid userId, CancellationToken ct = default)
    {
        var userPwd = await _uow.Repository<UserPassword>().Query()
            .FirstOrDefaultAsync(up => up.UserId == userId, ct);

        if (userPwd == null) throw new InvalidOperationException("User password record not found.");

        // Generate temporary password
        var rawTempPwd = Guid.NewGuid().ToString("N").Substring(0, 10);
        
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

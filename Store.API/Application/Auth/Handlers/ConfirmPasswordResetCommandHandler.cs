using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Store.API.Application.Abstractions;
using Store.API.Application.Auth.Requests;
using Store.DbServices.Context;

namespace Store.API.Application.Auth.Handlers;

public class ConfirmPasswordResetCommandHandler : IRequestHandler<ConfirmPasswordResetCommand, bool>
{
    private readonly StoreDbContext _context;

    public ConfirmPasswordResetCommandHandler(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HandleAsync(ConfirmPasswordResetCommand command, CancellationToken ct)
    {
        var request = command.Request;

        // Hash the provided token
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(request.Token));
        string tokenHash = Convert.ToBase64String(hashBytes);

        var tokenRecord = await _context.PasswordResetTokens
            .Include(t => t.User)
            .ThenInclude(u => u.Emails)
            .ThenInclude(e => e.Email)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        if (tokenRecord == null || tokenRecord.IsUsed || tokenRecord.ExpiryDate < DateTime.UtcNow)
        {
            return false; // Invalid or expired token
        }

        // Verify email matches
        if (!tokenRecord.User.Emails.Any(e => string.Equals(e.Email?.Address, request.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        // Update user's password
        var userPassword = await _context.UserPasswords.FirstOrDefaultAsync(p => p.UserId == tokenRecord.UserId, ct);
        
        if (userPassword == null)
        {
            userPassword = new Store.Models.Entities.UserPassword { UserId = tokenRecord.UserId };
            _context.UserPasswords.Add(userPassword);
        }

        userPassword.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        userPassword.ForcePasswordChange = false; // Reset the force flag if they had one

        // Invalidate token
        tokenRecord.IsUsed = true;
        tokenRecord.LastModified = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return true;
    }
}

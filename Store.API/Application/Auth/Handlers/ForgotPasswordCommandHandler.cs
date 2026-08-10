using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Store.API.Application.Abstractions;
using Store.API.Application.Auth.Requests;
using Store.DbServices.Context;
using Store.Models.Entities;
using Store.Models.Interfaces.Services;

namespace Store.API.Application.Auth.Handlers;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly StoreDbContext _context;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(StoreDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<bool> HandleAsync(ForgotPasswordCommand command, CancellationToken ct)
    {
        var request = command.Request;
        
        var user = await _context.Users
            .Include(u => u.Emails)
            .ThenInclude(e => e.Email)
            .FirstOrDefaultAsync(u => u.Username == request.UsernameOrEmail || 
                                      u.Emails.Any(e => e.Email.Address == request.UsernameOrEmail), ct);

        if (user == null)
        {
            // Return true even if user not found to prevent user enumeration attacks
            return true;
        }

        var primaryEmail = user.Emails.FirstOrDefault()?.Email?.Address;
        if (string.IsNullOrEmpty(primaryEmail))
        {
            // Cannot send email if user has no email address
            return true; 
        }

        // 1. Generate Token
        string rawToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        
        // 2. Hash Token for DB
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
        string tokenHash = Convert.ToBase64String(hashBytes);

        // 3. Save to DB
        var resetToken = new PasswordResetToken
        {
            UserId = user.UserId,
            TokenHash = tokenHash,
            ExpiryDate = DateTime.UtcNow.AddMinutes(30), // Valid for 30 mins
            IsUsed = false,
            DateCreated = DateTime.UtcNow,
            LastModified = DateTime.UtcNow
        };

        _context.PasswordResetTokens.Add(resetToken);
        await _context.SaveChangesAsync(ct);

        // 4. Send Email
        // Hardcoded domain for this internal app, ideally read from config
        string domain = "https://localhost:7258"; 
        string resetLink = $"{domain}/ResetPassword?token={rawToken}&email={Uri.EscapeDataString(primaryEmail)}";
        
        await _emailService.SendPasswordResetEmailAsync(primaryEmail, resetLink, ct);

        return true;
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Store.Models.DTOs.Auth;
using Store.Models.Entities;
using Store.Models.Entities.Contacts;
using Store.Models.DTOs.Operations;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Repositories;
using Store.Models.Interfaces.Services;
using Store.Models.Security;

namespace Store.DbServices.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _config;
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;
    // SEC-23 — device binding. Both nullable so the service stays
    // constructable in tests / hosts that have not yet wired the device
    // layer; when null, the binding checks fail open.
    private readonly ITrustedDeviceService? _trustedDevices;
    private readonly IDeviceBindingGuard? _deviceGuard;
    private readonly ILogger<AuthenticationService>? _logger;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticationService(
        IUnitOfWork uow,
        IConfiguration config,
        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor,
        ITrustedDeviceService? trustedDevices = null,
        IDeviceBindingGuard? deviceGuard = null,
        ILogger<AuthenticationService>? logger = null,
        IPasswordHasher? passwordHasher = null)
    {
        _uow = uow;
        _config = config;
        _httpContextAccessor = httpContextAccessor;
        _trustedDevices = trustedDevices;
        _deviceGuard = deviceGuard;
        _logger = logger;
        _passwordHasher = passwordHasher ?? Argon2idPasswordHasher.Default;
    }

    /// <summary>
    /// SEC-23 — register-or-update the calling device after a successful
    /// login. Best-effort: a failure here must not block the user's
    /// sign-in (the device layer is observability + anomaly detection,
    /// not a hard gate on the password path).
    /// </summary>
    private async Task RegisterDeviceAsync(Guid userId, CancellationToken ct)
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (_trustedDevices is null || ctx is null) return;

        try
        {
            await _trustedDevices.RegisterOrUpdateAsync(userId, ctx, ct);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "SEC-23 — device registration failed for user {UserId}", userId);
        }
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>().Query()
            .Include(u => u.Password)
            .Include(u => u.UserTokens)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username.Trim(), ct);

        return await AuthenticateUser(user, request.Password, ct);
    }

    public async Task<LoginResponse?> LoginWithEmailAsync(LoginWithEmailRequest request, CancellationToken ct = default)
    {
        var userEmail = await _uow.Repository<UserEmail>().Query()
            .Include(ue => ue.Email)
            .FirstOrDefaultAsync(ue => ue.Email.Address == request.Email.Trim().ToLowerInvariant(), ct);

        if (userEmail is null) return null;

        var user = await _uow.Repository<User>().Query()
            .Include(u => u.Password)
            .Include(u => u.UserTokens)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userEmail.UserId, ct);

        return await AuthenticateUser(user, request.Password, ct);
    }

    public async Task<LoginResponse?> LoginWithPhoneAsync(LoginWithPhoneRequest request, CancellationToken ct = default)
    {
        var userPhone = await _uow.Repository<UserPhone>().Query()
            .Include(up => up.Phone)
            .FirstOrDefaultAsync(up => up.Phone.Number == request.Phone.Trim(), ct);

        if (userPhone is null) return null;

        var user = await _uow.Repository<User>().Query()
            .Include(u => u.Password)
            .Include(u => u.UserTokens)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userPhone.UserId, ct);

        return await AuthenticateUser(user, request.Password, ct);
    }

    public async Task<LoginResponse?> LoginWithBiometricsAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>().Query()
            .Include(u => u.Role)
            .Include(u => u.UserTokens)
            .Include(u => u.Password)
            .FirstOrDefaultAsync(u => u.UserId == userId, ct);

        if (user is null || user.Status == UserStatus.Banned || user.Status == UserStatus.Deleted)
            return null;

        // Check if account is locked out
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            return new LoginResponse
            {
                IsLockedOut = true,
                LockoutRemainingMinutes = (int)Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes)
            };
        }

        // On successful biometric login, clear any lockout
        if (user.FailedLoginAttempts > 0 || user.LockoutEnd.HasValue)
        {
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            _uow.Repository<User>().Update(user);
            await _uow.SaveChangesAsync(ct);
        }

        if (user.Password != null && (user.Password.ForcePasswordChange || (user.Password.TempPasswordExpiresAt.HasValue && user.Password.TempPasswordExpiresAt.Value < DateTime.UtcNow)))
        {
            return new LoginResponse
            {
                RequiresPasswordReset = true,
                User = new AuthenticatedUserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Role = user.Role.Name,
                    ThumbnailUrl = user.ThumbnailUrl
                }
            };
        }

        // Skip password verification and just issue the tokens
        var permissions = await GetPermissionClaimsAsync(user.RoleId, ct);
        var (token, refreshToken, expiry, refreshExpiry) = GenerateTokens(user, permissions);

        var context = _httpContextAccessor.HttpContext;
        var ipAddress = context?.Connection?.RemoteIpAddress?.ToString();
        var userAgent = context?.Request?.Headers["User-Agent"].ToString();

        var newToken = new UserToken
        {
            UserId = user.UserId,
            Token = token,
            RefreshTokenHash = HashRefreshToken(refreshToken),
            ExpiryDate = expiry,
            RefreshTokenExpiryDate = refreshExpiry,
            IsRevoked = false,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DateCreated = DateTime.UtcNow,
            LastActive = DateTime.UtcNow
        };
        
        user.UserTokens.Add(newToken);
        await _uow.SaveChangesAsync(ct);

        return new LoginResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            AccessTokenExpiry = expiry,
            RefreshTokenExpiry = refreshExpiry,
            User = new AuthenticatedUserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role.Name,
                ThumbnailUrl = user.ThumbnailUrl
            }
        };
    }

    public async Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        // SEC-28 — controller guarantees RefreshToken is non-null; the access
        // token is optional (used only for audit / user identification).
        var accessToken = request.Token ?? string.Empty;
        var refreshToken = request.RefreshToken ?? string.Empty;
        if (string.IsNullOrWhiteSpace(refreshToken)) return null;

        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal is null) return null;

        var userIdClaim = principal.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return null;

        var userToken = await _uow.Repository<UserToken>().Query()
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == accessToken && !t.IsRevoked, ct);

        if (userToken is null) return null;
        if (userToken.RefreshTokenExpiryDate < DateTime.UtcNow) return null;

        var refreshHash = HashRefreshToken(refreshToken);
        if (!CryptographicEquals(userToken.RefreshTokenHash, refreshHash)) return null;

        var user = await _uow.Repository<User>().Query()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId, ct);

        if (user is null || user.Status != UserStatus.Active) return null;

        // SEC-23 — device-binding gate. A refresh token presented from a
        // device the user has never used (token stolen / shared-device
        // replay) is rejected before we mint a fresh session. Password-
        // only users still pass (LastWebAuthnAtUtc == null → PasswordOnly
        // is allowed); only UnknownDevice / RevokedDevice block.
        if (_deviceGuard is not null && _httpContextAccessor.HttpContext is { } ctx)
        {
            var binding = await _deviceGuard.CheckAsync(userId, ctx, ct);
            if (binding == DeviceBindingResult.UnknownDevice || binding == DeviceBindingResult.RevokedDevice)
            {
                _logger?.LogWarning(
                    "SEC-23 — refresh denied for user {UserId}: device binding verdict {Verdict}",
                    userId, binding);
                return null;
            }
        }

        var permissions = await GetPermissionClaimsAsync(user.RoleId, ct);
        var (token, newRefresh, expiry, refreshExpiry) = GenerateTokens(user, permissions);

        // SEC-14 — atomic compare-and-set on the OLD token value.
        // Two concurrent refreshes both read the same Token=X. The first UPDATE
        // flips Token from X to Y (and revokes X via the WHERE). The second
        // UPDATE matches 0 rows because Token is no longer X — that's the race
        // loser and we return null so it can re-authenticate.
        var repo = _uow.Repository<UserToken>();
        var newRefreshHash = HashRefreshToken(newRefresh);
        var rowsAffected = await repo.Query()
            .Where(t => t.UserTokenId == userToken.UserTokenId && t.Token == request.Token && !t.IsRevoked)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.Token, token)
                .SetProperty(t => t.RefreshTokenHash, newRefreshHash)
                .SetProperty(t => t.ExpiryDate, expiry)
                .SetProperty(t => t.RefreshTokenExpiryDate, refreshExpiry)
                .SetProperty(t => t.IsRevoked, false), ct);

        if (rowsAffected == 0)
        {
            // Concurrent refresh won. The AuditLoggingMiddleware will pick up
            // the resulting 401 from the next API call and log the event.
            return null;
        }

        // SEC-23 — register-or-update the calling device so the next
        // refresh sees this fingerprint as known.
        await RegisterDeviceAsync(user.UserId, ct);

        return new LoginResponse
        {
            AccessToken = token,
            RefreshToken = newRefresh,
            AccessTokenExpiry = expiry,
            RefreshTokenExpiry = refreshExpiry,
            User = new AuthenticatedUserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role.Name,
                ThumbnailUrl = user.ThumbnailUrl
            }
        };
    }
    public async Task<LoginResponse?> Login2FAAsync(Login2FARequest request, CancellationToken ct = default)
    {
        var principal = GetPrincipalFromTempToken(request.TwoFactorToken);
        if (principal is null) return null;

        if (principal.FindFirst("2fa_pending")?.Value != "true")
            return null;

        var userIdClaim = principal.FindFirst("uid")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return null;

        var user = await _uow.Repository<User>().Query()
            .Include(u => u.Role)
            .Include(u => u.UserTokens)
            .FirstOrDefaultAsync(u => u.UserId == userId, ct);

        if (user is null || user.Status != UserStatus.Active || !user.TwoFactorEnabled || string.IsNullOrWhiteSpace(user.TwoFactorSecret))
            return null;

        // Verify the code
        var totp = new OtpNet.Totp(OtpNet.Base32Encoding.ToBytes(user.TwoFactorSecret));
        var isValid = totp.VerifyTotp(request.Code, out long timeStepMatched, window: new OtpNet.VerificationWindow(2, 2));

        if (!isValid)
            return null;

        // Code is valid, issue real tokens
        var permissions = await GetPermissionClaimsAsync(user.RoleId, ct);
        var (token, refreshToken, expiry, refreshExpiry) = GenerateTokens(user, permissions);

        var context = _httpContextAccessor.HttpContext;
        var ipAddress = context?.Connection?.RemoteIpAddress?.ToString();
        var userAgent = context?.Request?.Headers["User-Agent"].ToString();

        var newToken = new UserToken
        {
            UserId = user.UserId,
            Token = token,
            RefreshTokenHash = HashRefreshToken(refreshToken),
            ExpiryDate = expiry,
            RefreshTokenExpiryDate = refreshExpiry,
            IsRevoked = false,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DateCreated = DateTime.UtcNow,
            LastActive = DateTime.UtcNow
        };
        
        user.UserTokens.Add(newToken);
        await _uow.SaveChangesAsync(ct);

        return new LoginResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            AccessTokenExpiry = expiry,
            RefreshTokenExpiry = refreshExpiry,
            User = new AuthenticatedUserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role.Name,
                ThumbnailUrl = user.ThumbnailUrl
            }
        };
    }

    public async Task<bool> LogoutAsync(Guid userId, CancellationToken ct = default)
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].FirstOrDefault();
        var token = authHeader?.StartsWith("Bearer ") == true ? authHeader.Substring("Bearer ".Length).Trim() : null;

        if (string.IsNullOrEmpty(token)) return false;

        var userToken = await _uow.Repository<UserToken>().Query()
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Token == token, ct);

        if (userToken is not null)
        {
            userToken.IsRevoked = true;
            _uow.Repository<UserToken>().Update(userToken);
            await _uow.SaveChangesAsync(ct);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Revokes all active sessions for a specific user and forces them to re-authenticate everywhere.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if successful, otherwise false.</returns>
    public async Task<bool> RevokeAllSessionsAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>().Query()
            .FirstOrDefaultAsync(u => u.UserId == userId, ct);
            
        if (user is null) return false;

        // Changing the security stamp invalidates all existing stateless JWTs
        user.SecurityStamp = Guid.NewGuid();
        _uow.Repository<User>().Update(user);

        // Also revoke all active refresh tokens
        var userTokens = await _uow.Repository<UserToken>().Query()
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(ct);

        foreach (var t in userTokens)
        {
            t.IsRevoked = true;
            _uow.Repository<UserToken>().Update(t);
        }

        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>().Query()
            .Include(u => u.Password)
            .FirstOrDefaultAsync(u => u.Username == request.Username.Trim(), ct);

        if (user?.Password is null) return false;

        // Verify old password before allowing reset
        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.Password.PasswordHash))
            return false;

        user.Password.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.Password.LastModified = DateTime.UtcNow;
        _uow.Repository<UserPassword>().Update(user.Password);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    // -----------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------

    private async Task<LoginResponse?> AuthenticateUser(User? user, string password, CancellationToken ct)
    {
        if (user is null) return null;
        if (user.Password is null) return null;
        if (user.Status == UserStatus.Banned || user.Status == UserStatus.Deleted) return null;

        // Check if account is locked out
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            return new LoginResponse
            {
                IsLockedOut = true,
                LockoutRemainingMinutes = (int)Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes)
            };
        }

        if (!_passwordHasher.VerifyPassword(password, user.Password.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            }
            _uow.Repository<User>().Update(user);
            await _uow.SaveChangesAsync(ct);
            return null;
        }

        // SEC-24 — Transparent re-hash to Argon2id if user still has legacy BCrypt hash
        if (_passwordHasher.NeedsRehash(user.Password.PasswordHash))
        {
            user.Password.PasswordHash = _passwordHasher.HashPassword(password);
            user.Password.LastModified = DateTime.UtcNow;
            _uow.Repository<UserPassword>().Update(user.Password);
            _logger?.LogInformation("SEC-24: Password hash seamlessly upgraded to Argon2id for user {UserId}", user.UserId);
        }

        // On successful password, clear any lockout
        if (user.FailedLoginAttempts > 0 || user.LockoutEnd.HasValue)
        {
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            _uow.Repository<User>().Update(user);
            await _uow.SaveChangesAsync(ct);
        }

        if (user.Password.ForcePasswordChange || (user.Password.TempPasswordExpiresAt.HasValue && user.Password.TempPasswordExpiresAt.Value < DateTime.UtcNow))
        {
            return new LoginResponse
            {
                RequiresPasswordReset = true,
                User = new AuthenticatedUserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Role = user.Role.Name,
                    ThumbnailUrl = user.ThumbnailUrl,
                    FullImageUrl = user.FullImageUrl
                }
            };
        }

        if (user.TwoFactorEnabled)
        {
            return new LoginResponse
            {
                RequiresTwoFactor = true,
                TwoFactorToken = Generate2FATempToken(user.UserId),
                User = new AuthenticatedUserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Role = user.Role.Name,
                    ThumbnailUrl = user.ThumbnailUrl,
                    FullImageUrl = user.FullImageUrl
                }
            };
        }

        var permissions = await GetPermissionClaimsAsync(user.RoleId, ct);
        var (token, refreshToken, expiry, refreshExpiry) = GenerateTokens(user, permissions);

        var context = _httpContextAccessor.HttpContext;
        var ipAddress = context?.Connection?.RemoteIpAddress?.ToString();
        var userAgent = context?.Request?.Headers["User-Agent"].ToString();

        var newToken = new UserToken
        {
            UserId = user.UserId,
            Token = token,
            RefreshTokenHash = HashRefreshToken(refreshToken),
            ExpiryDate = expiry,
            RefreshTokenExpiryDate = refreshExpiry,
            IsRevoked = false,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DateCreated = DateTime.UtcNow,
            LastActive = DateTime.UtcNow
        };
        
        user.UserTokens.Add(newToken);

        await _uow.SaveChangesAsync(ct);

        // SEC-23 — register the calling device so subsequent refreshes
        // can detect replay-from-an-unknown-device.
        await RegisterDeviceAsync(user.UserId, ct);

        return new LoginResponse
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            AccessTokenExpiry = expiry,
            RefreshTokenExpiry = refreshExpiry,
            User = new AuthenticatedUserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role.Name,
                ThumbnailUrl = user.ThumbnailUrl
            }
        };
    }

    private (string Token, string RefreshToken, DateTime Expiry, DateTime RefreshExpiry) GenerateTokens(
        User user,
        IEnumerable<string> permissions)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured.");
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];
        var accessMinutes = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "60");
        var refreshDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7");

        var expiry = DateTime.UtcNow.AddMinutes(accessMinutes);
        var refreshExpiry = DateTime.UtcNow.AddDays(refreshDays);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claimList = new List<Claim>
        {
            new("uid", user.UserId.ToString()),
            new(ClaimTypes.Role, user.Role.Name),
            new(JwtRegisteredClaimNames.Sub, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new("stamp", user.SecurityStamp.ToString())
        };

        // GAP-04 — surface ForcePasswordChange so the Razor Pages UI can short-circuit
        // to /ForceResetPassword without an extra /me round-trip on every page.
        // The claim is regenerated on each login / refresh, so once the user resets
        // their password the next login removes the flag.
        if (user.Password is { ForcePasswordChange: true })
        {
            claimList.Add(new Claim("force_password_change", "true"));
        }

        foreach (var permission in permissions)
        {
            claimList.Add(new Claim("perm", permission));
        }

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claimList,
            expires: expiry,
            signingCredentials: creds);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        var refreshToken = GenerateSecureRefreshToken();

        return (token, refreshToken, expiry, refreshExpiry);
    }

    private async Task<IReadOnlyList<string>> GetPermissionClaimsAsync(int roleId, CancellationToken ct)
    {
        var explicitPerms = await _uow.Repository<RolePermission>().Query()
            .AsNoTracking()
            .Where(x => x.RoleId == roleId && x.IsAllowed)
            .Select(x => x.PermissionKey)
            .ToListAsync(ct);

        if (explicitPerms.Count > 0)
        {
            return explicitPerms;
        }

        var roleName = await _uow.Repository<Role>().Query()
            .AsNoTracking()
            .Where(x => x.RoleId == roleId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(ct) ?? string.Empty;

        if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            return PermissionKeys.All;
        }

        var fallback = new List<string>();
        if (roleName.Equals("Manager", StringComparison.OrdinalIgnoreCase))
        {
            fallback.AddRange(new[]
            {
                PermissionKeys.InventoryRead,
                PermissionKeys.InventoryWrite,
                PermissionKeys.PricingRead,
                PermissionKeys.PricingWrite,
                PermissionKeys.CashRead,
                PermissionKeys.CashWrite,
                PermissionKeys.ReportsRead
            });
        }

        if (roleName.Equals("Cashier", StringComparison.OrdinalIgnoreCase))
        {
            fallback.AddRange(new[]
            {
                PermissionKeys.CashRead,
                PermissionKeys.CashWrite
            });
        }

        return fallback;
    }

    private static string GenerateSecureRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashRefreshToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    private static bool CryptographicEquals(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }

    private ClaimsPrincipal? GetPrincipalFromTempToken(string token)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured.");
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
            ValidateIssuer = !string.IsNullOrEmpty(issuer),
            ValidIssuer = issuer,
            ValidateAudience = !string.IsNullOrEmpty(audience),
            ValidAudience = audience,
            ValidateLifetime = true // We want it to fail if expired!
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private string Generate2FATempToken(Guid userId)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured.");
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("uid", userId.ToString()),
                new Claim("2fa_pending", "true")
            }),
            Expires = DateTime.UtcNow.AddMinutes(5),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtKey = _config["Jwt:Key"];
        if (jwtKey is null) return null;

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false  // We intentionally accept expired tokens here
        };

        try
        {
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token, validationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }
}

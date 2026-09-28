using Microsoft.AspNetCore.Http;

namespace Store.API.Auth;

/// <summary>
/// SEC-19 — HttpOnly Secure SameSite=Strict cookie issuance for JWT auth.
///
/// <para>
/// The default token transport (a JSON body field) leaves the access token
/// readable by any script running on the page (XSS-theft surface). Cookies
/// flagged <c>HttpOnly</c> are inaccessible to JavaScript, so even a successful
/// XSS injection cannot exfiltrate them. <c>Secure</c> ensures they only ride
/// HTTPS. <c>SameSite=Strict</c> blocks CSRF — the cookie is never sent on a
/// cross-site navigation, so a malicious page cannot trigger state-changing
/// calls under the victim's session.
/// </para>
///
/// <para>
/// The browser sends these cookies automatically on every same-origin request
/// to the API, so the API also accepts them as a Bearer-token source — see
/// the cookie-reading branch in <c>Program.cs</c> JWT options. Tokens still
/// appear in the JSON body for non-browser clients (mobile apps, integrations).
/// </para>
/// </summary>
public static class AuthCookieHelper
{
    public const string AccessTokenCookieName = "store_at";
    public const string RefreshTokenCookieName = "store_rt";

    /// <summary>Emit both access + refresh cookies on a successful auth response.</summary>
    public static void SetAuthCookies(
        this HttpResponse response,
        string accessToken,
        DateTime accessTokenExpiry,
        string refreshToken,
        DateTime refreshTokenExpiry,
        bool isSecureContext)
    {
        var now = DateTime.UtcNow;

        response.Cookies.Append(AccessTokenCookieName, accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecureContext,
            SameSite = SameSiteMode.Strict,
            Expires = accessTokenExpiry,
            Path = "/",
            IsEssential = true
        });

        response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecureContext,
            SameSite = SameSiteMode.Strict,
            Expires = refreshTokenExpiry,
            Path = "/",
            IsEssential = true
        });
    }

    /// <summary>Drop both auth cookies (logout). Path must match the issuance path.</summary>
    public static void ClearAuthCookies(this HttpResponse response)
    {
        response.Cookies.Delete(AccessTokenCookieName, new CookieOptions { Path = "/" });
        response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions { Path = "/" });
    }
}

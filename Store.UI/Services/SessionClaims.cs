using System.Text;
using System.Text.Json;

namespace StoreUI.Services;

/// <summary>
/// Reads the JWT access token from <c>HttpContext.Session["access_token"]</c>
/// and surfaces the security claims without an extra round-trip to the API
/// or a JWT-handler package dependency.
/// </summary>
/// <remarks>
/// The store-project uses session-backed JWT for the UI (Wave 12 deferred
/// item SEC-19 — replacing session with an HttpOnly cookie).
/// Until then, this helper centralises base64url-claims-reading so
/// feature code can simply ask "is this user forced to reset their password?"
/// instead of re-parsing on every page.
/// A JWT is just three base64url'd JSON segments; we only need the
/// payload (middle segment) and only its name/value claim pairs.
/// </remarks>
public static class SessionClaims
{
    /// <summary>
    /// SEC-19 (partial) — fast check: does the current session's JWT carry a
    /// <c>force_password_change=true</c> claim? Used by POS, POS check-out,
    /// and any other high-trust page to short-circuit to <c>/ForceResetPassword</c>.
    /// </summary>
    public static bool IsForcePasswordChangeRequired(ISession session)
    {
        return string.Equals(
            Get(session, "force_password_change"),
            "true",
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// SEC-19 — read a single string claim from the session JWT, if present.
    /// Returns null if the session has no token or the claim is absent.
    /// Silently swallows decode errors (malformed tokens are rejected at the
    /// API on the next call, so we don't gate the UI here).
    /// </summary>
    public static string? Get(ISession session, string claimType)
    {
        var jwt = session.GetString("access_token");
        if (string.IsNullOrWhiteSpace(jwt)) return null;

        try
        {
            // JWT = base64url(header).base64url(payload).base64url(signature)
            // We only need the payload to read claims; the signature is
            // verified by the API on every call.
            var segments = jwt.Split('.');
            if (segments.Length < 2) return null;

            var payload = Base64UrlDecode(segments[1]);
            using var doc = JsonDocument.Parse(payload);

            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (string.Equals(prop.Name, claimType, StringComparison.Ordinal))
                {
                    return prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString(),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        JsonValueKind.Number => prop.Value.GetRawText(),
                        _ => prop.Value.GetRawText()
                    };
                }
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    private static byte[] Base64UrlDecode(string s)
    {
        // RFC 7515 §3 — base64url: replace - / _ with + / , and add padding.
        var padded = s.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        return Convert.FromBase64String(padded);
    }
}

using System.Text;
using System.Text.Json;

namespace StoreUI.Services;

public static class JwtPermissionReader
{
    public static HashSet<string> GetPermissions(string? jwt)
    {
        var perms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(jwt))
        {
            return perms;
        }

        var parts = jwt.Split('.');
        if (parts.Length < 2)
        {
            return perms;
        }

        try
        {
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));

            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("perm", out var permNode))
            {
                return perms;
            }

            if (permNode.ValueKind == JsonValueKind.Array)
            {
                foreach (var value in permNode.EnumerateArray())
                {
                    var raw = value.GetString();
                    if (!string.IsNullOrWhiteSpace(raw))
                    {
                        perms.Add(raw);
                    }
                }
            }
            else if (permNode.ValueKind == JsonValueKind.String)
            {
                var raw = permNode.GetString();
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    perms.Add(raw);
                }
            }
        }
        catch
        {
            return perms;
        }

        return perms;
    }

    public static string? GetClaim(string? jwt, string claimName)
    {
        if (string.IsNullOrWhiteSpace(jwt)) return null;

        var parts = jwt.Split('.');
        if (parts.Length < 2) return null;

        try
        {
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));

            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty(claimName, out var node))
                return node.GetString();
        }
        catch { }

        return null;
    }
}

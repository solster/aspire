namespace Aspire.Hosting.Azure;

internal static class AccessTokenClaims
{
    public static string? GetUsername(string accessToken)
    {
        var segments = accessToken.Split('.');
        if (segments.Length != 3)
        {
            return null;
        }

        using var doc = ReadPayload(segments[1]);
        if (doc is null)
        {
            return null;
        }

        if (doc.RootElement.TryGetProperty("upn", out var upn))
        {
            return GetString(upn);
        }

        if (doc.RootElement.TryGetProperty("preferred_username", out var preferredUsername))
        {
            return GetString(preferredUsername);
        }

        return null;
    }

    private static string? GetString(System.Text.Json.JsonElement element)
    {
        return element.ValueKind == System.Text.Json.JsonValueKind.String
            ? element.GetString()
            : null;
    }

    private static System.Text.Json.JsonDocument? ReadPayload(string jwtPayload)
    {
        try
        {
            jwtPayload = jwtPayload
                .Replace('-', '+')
                .Replace('_', '/');
            var padded = jwtPayload.PadRight(jwtPayload.Length + (4 - jwtPayload.Length % 4) % 4, '=');
            var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));

            return System.Text.Json.JsonDocument.Parse(json);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }
}

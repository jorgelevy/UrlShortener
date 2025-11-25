using Microsoft.AspNetCore.Http;

namespace UrlShortener.Common.Helpers;

public static class HttpRequestHelper
{
    // Gets the client IP address. Checks common proxy headers first (X-Forwarded-For, X-Real-IP),
    // then falls back to the connection remote IP.
    public static string GetClientIp(HttpRequest req)
    {
        if (req.Headers.TryGetValue("X-Forwarded-For", out var xff) && !string.IsNullOrWhiteSpace(xff))
        {
            // X-Forwarded-For may contain a comma-separated list of IPs; use the first one
            var first = xff.ToString().Split(',').Select(s => s.Trim()).FirstOrDefault();
            if (!string.IsNullOrEmpty(first))
                return first;
        }

        if (req.Headers.TryGetValue("X-Real-IP", out var xr) && !string.IsNullOrWhiteSpace(xr))
        {
            return xr.ToString();
        }

        var ip = req.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        return ip ?? "Unknown";
    }
}

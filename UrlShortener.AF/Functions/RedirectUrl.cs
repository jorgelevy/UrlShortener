using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace UrlShortener.AF.Functions;

public class RedirectUrl
{
    private readonly ILogger<RedirectUrl> _logger;


    // Simple in-memory store (replace with persistent storage)
    private static readonly Dictionary<string, string> UrlStore = new Dictionary<string, string> {
        { "jl", "https://www.jorgelevy.net" }
};

    public RedirectUrl(ILogger<RedirectUrl> logger)
    {
        _logger = logger;
    }

    [Function("RedirectUrl")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{code}")] HttpRequest req, string code)
    {
        var clientIp = GetClientIp(req);
        _logger.LogInformation("Redirect request for code {Code} from IP {ClientIp}", code, clientIp);

        if (UrlStore.TryGetValue(code, out var originalUrl))
        {
            return new RedirectResult(originalUrl);
        }
        return new NotFoundObjectResult("Short URL not found.");

    }

    // Gets the client IP address. Checks common proxy headers first (X-Forwarded-For, X-Real-IP),
    // then falls back to the connection remote IP.
    private static string GetClientIp(HttpRequest req)
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
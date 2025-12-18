using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using UrlShortener.Common.Helpers;

namespace UrlShortener.AF.Functions;

public class RedirectUrl
{
    private readonly ILogger<RedirectUrl> _logger;


    // Simple in-memory store (replace with persistent storage)
    private static readonly Dictionary<string, string> UrlStore = new Dictionary<string, string> {
        { "jl", "https://www.jorgelevy.net" },
        { "csadvent-christmas-2025", "https://jorgelevy.net/posts/create-your-mcp-server-using-azure-functions/" },
        { "diciembre-de-agentes-2025", "https://jorgelevy.net/posts/crea-tus-servicios-mcp-con-azure-functions/" },
        { "calendario-adviento-2025", "https://jorgelevy.net/posts/aspire-mas-alla-de-net/" },
    };

    public RedirectUrl(ILogger<RedirectUrl> logger)
    {
        _logger = logger;
    }

    [Function("RedirectUrl")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{code}")] HttpRequest req, string code)
    {
        var clientIp = HttpRequestHelper.GetClientIp(req);
        _logger.LogInformation("Redirect request for code {Code} from IP {ClientIp}", code, clientIp);

        if (UrlStore.TryGetValue(code, out var originalUrl))
        {
            return new RedirectResult(originalUrl);
        }
        return new NotFoundObjectResult("Short URL not found.");
    }
}
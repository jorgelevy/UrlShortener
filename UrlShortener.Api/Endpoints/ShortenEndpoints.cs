namespace UrlShortener.Api.Endpoints;

public static class ShortenEndpoints
{
    public static void MapShortenEndpoints(this WebApplication app)
    {
        var urlStore = new Dictionary<string, string>
        {
            { "jl", "https://www.jorgelevy.net" }
        };

        var shortenGroup = app.MapGroup("/api/shorten").WithTags("URL Shortening");

        shortenGroup.MapGet("/{url}", (HttpContext context, string url) =>
        {
            if (string.IsNullOrWhiteSpace(url))
                return Results.BadRequest("URL is required");

            var shortCode = Guid.NewGuid().ToString().Substring(0, 6);
            urlStore[shortCode] = url;

            return Results.Ok(new { shortUrl = $"{context.Request.Scheme}://{context.Request.Host}/{shortCode}" });
        });

        shortenGroup.MapPost("", async (HttpContext context) =>
        {
            var form = await context.Request.ReadFormAsync();
            var originalUrl = form["url"].ToString();

            if (string.IsNullOrWhiteSpace(originalUrl))
                return Results.BadRequest("URL is required");

            var shortCode = Guid.NewGuid().ToString().Substring(0, 6);
            urlStore[shortCode] = originalUrl;

            return Results.Ok(new { shortUrl = $"{context.Request.Scheme}://{context.Request.Host}/{shortCode}" });
        });

        ////app.MapPost("/shorten", async (HttpContext httpContext, IUrlShorteningService urlShorteningService, ILogger<Program> logger) =>
        //shortenGroup.MapPost("/shorten", async (HttpContext httpContext, ILogger<Program> logger) =>
        //{
        //    try
        //    {
        //        var request = await httpContext.Request.ReadFromJsonAsync<ShortenUrlRequest>();
        //        if (request == null || string.IsNullOrEmpty(request.OriginalUrl))
        //        {
        //            return Results.BadRequest("Invalid request payload.");
        //        }
        //        var shortUrl = await urlShorteningService.ShortenUrlAsync(request.OriginalUrl);
        //        var response = new ShortenUrlResponse { ShortUrl = shortUrl };
        //        return Results.Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Error occurred while shortening URL.");
        //        return Results.StatusCode(500);
        //    }
        //})
        //.WithName("ShortenUrl")
        //.WithTags("URL Shortening");
    }
}

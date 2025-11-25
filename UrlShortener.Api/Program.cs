var urlStore = new Dictionary<string, string>
{
    { "jl", "https://www.jorgelevy.net" }
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/shorten/{url}", (HttpContext context, string url) =>
{
    if (string.IsNullOrWhiteSpace(url))
        return Results.BadRequest("URL is required");

    var shortCode = Guid.NewGuid().ToString().Substring(0, 6);
    urlStore[shortCode] = url;

    return Results.Ok(new { shortUrl = $"{context.Request.Scheme}://{context.Request.Host}/{shortCode}" });
});

app.MapPost("/shorten", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var originalUrl = form["url"].ToString();

    if (string.IsNullOrWhiteSpace(originalUrl))
        return Results.BadRequest("URL is required");

    var shortCode = Guid.NewGuid().ToString().Substring(0, 6);
    urlStore[shortCode] = originalUrl;

    return Results.Ok(new { shortUrl = $"{context.Request.Scheme}://{context.Request.Host}/{shortCode}" });
});

// Redirect endpoint
app.MapGet("/{code}", (HttpContext context, string code) =>
{
    var clientIp = GetClientIp(context.Request);
    Console.WriteLine($"Redirect request for {code} from {clientIp}");

    if (urlStore.TryGetValue(code, out var originalUrl))
    {
        return Results.Redirect(originalUrl);
    }

    return Results.NotFound("Invalid short code");
});

app.Run();


// Gets the client IP address. Checks common proxy headers first (X-Forwarded-For, X-Real-IP),
// then falls back to the connection remote IP.
static string GetClientIp(HttpRequest req)
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
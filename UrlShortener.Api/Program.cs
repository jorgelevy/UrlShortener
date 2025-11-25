using UrlShortener.Common.Helpers;

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
    var clientIp = HttpRequestHelper.GetClientIp(context.Request);
    Console.WriteLine($"Redirect request for {code} from {clientIp}");

    if (urlStore.TryGetValue(code, out var originalUrl))
    {
        return Results.Redirect(originalUrl);
    }

    return Results.NotFound("Invalid short code");
});

app.Run();
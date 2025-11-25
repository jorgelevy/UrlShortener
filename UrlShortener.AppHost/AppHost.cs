var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.UrlShortener_Api>("urlshortener-api");

builder.AddAzureFunctionsProject<Projects.UrlShortener_AF>("urlshortener-af");

builder.Build().Run();

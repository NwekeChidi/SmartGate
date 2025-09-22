using SmartGate.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o => { o.TimestampFormat = "HH:mm:ss "; o.SingleLine = true; });

builder.Services
    .AddDatabase(builder.Configuration, builder.Environment)
    .AddApplicationServices()
    .AddSmartGateAuth(builder.Configuration)
    .AddApiServices()
    .AddSwaggerDocumentation()
    .AddRateLimiting(builder.Configuration)
    .AddHealthChecks();

var app = builder.Build();

app.ConfigurePipeline().Run();

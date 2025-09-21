using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmartGate.Api.Auth;
using SmartGate.Application.Abstractions;
using SmartGate.Application.Visits;
using SmartGate.Application.Visits.Dto;
using SmartGate.Application.Visits.Ports;
using SmartGate.Infrastructure.Database;
using SmartGate.Infrastructure.Repositories;
using System.Text.Json.Serialization;
using FluentValidation;
using SmartGate.Api.ErrorHandling;
using Microsoft.AspNetCore.Mvc.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o => { o.TimestampFormat = "HH:mm:ss "; o.SingleLine = true; });

// DbContext
builder.Services.AddDbContext<SmartGateDbContext>(o =>
{
    var cs = builder.Configuration.GetConnectionString("Postgres");
    var isDev = builder.Environment.IsDevelopment();
    o.UseNpgsql(cs)
     .EnableDetailedErrors(isDev)
     .EnableSensitiveDataLogging(isDev);
});

// DI: Application + Infra
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, HttpUserContext>();
builder.Services.AddScoped<IClock, SystemClock>();
builder.Services.AddScoped<IPiiPolicy, PassthroughPiiPolicy>();
builder.Services.AddScoped<IVisitRepository, VisitRepository>();
builder.Services.AddScoped<IIdempotencyStore, IdempotencyStore>();
builder.Services.AddScoped<IValidator<CreateVisitRequest>, CreateVisitRequestValidator>();
builder.Services.AddScoped<IVisitService, VisitService>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddSingleton<ProblemDetailsFactory, FlatErrorsProblemDetailsFactory>();

// Auth
var useDev = builder.Configuration.GetValue<bool>("Auth:UseDevAuth");
var jwtAuthority = builder.Configuration["Jwt:Authority"];
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"];



if (useDev)
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = DevAuthHandler.Scheme;
        options.DefaultChallengeScheme = DevAuthHandler.Scheme;
    }).AddScheme<DevAuthOptions, DevAuthHandler>(DevAuthHandler.Scheme, _ => { });
}
else
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new()
            {
                ValidateIssuer = !string.IsNullOrWhiteSpace(jwtAuthority),
                ValidIssuer = jwtAuthority,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(jwtSigningKey ?? throw new InvalidOperationException("[Auth] JWT SigningKey is required for non-dev environments"))),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                NameClaimType = "sub"
            };
            options.Events = new JwtBearerEvents
            {
                OnChallenge = context =>
                {
                    Console.WriteLine($"[Auth] JWT Challenge: {context.Error}, {context.ErrorDescription}");
                    return Task.CompletedTask;
                }
            };
        });
}

// Authorization policies
var authBuilder = builder.Services.AddAuthorizationBuilder()
    .AddPolicy("VisitsRead", policy =>
        policy.RequireAssertion(ctx => Policies.HasScopeOrRole(ctx.User, Policies.ReadScope)))
    .AddPolicy("VisitsWrite", policy =>
        policy.RequireAssertion(ctx => Policies.HasScopeOrRole(ctx.User, Policies.WriteScope)));

// Require authentication for all endpoints in non-dev environments
if (!useDev)
{
    authBuilder.AddFallbackPolicy("RequireAuth", policy => policy.RequireAuthenticatedUser());
}

// Controllers + ProblemDetails + Swagger
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = null;
    o.JsonSerializerOptions.Converters.Add(new StringTryParseConverterFactory());
    o.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(allowIntegerValues: false));
    o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartGate API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});


// Rate limiting
var perMinute = builder.Configuration.GetValue("RateLimiting:PermitPerMinute", 120);
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = perMinute;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = perMinute;
    }));

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseRateLimiter();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();

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
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o => { o.TimestampFormat = "HH:mm:ss "; o.SingleLine = true; });

// DbContext
builder.Services.AddDbContext<SmartGateDbContext>(o =>
{
    var cs = builder.Configuration.GetConnectionString("Postgres");
    o.UseNpgsql(cs)
     .EnableDetailedErrors()
     .EnableSensitiveDataLogging(); // dev only
});

// DI: Application + Infra
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, HttpUserContext>();
builder.Services.AddScoped<IClock, SystemClock>();
builder.Services.AddScoped<IPiiPolicy, PassthroughPiiPolicy>();
builder.Services.AddScoped<IVisitRepository, VisitRepository>();
builder.Services.AddScoped<IIdempotencyStore, EfIdempotencyStore>();
builder.Services.AddScoped<IValidator<CreateVisitRequest>, CreateVisitRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateVisitStatusRequest>, UpdateVisitStatusRequestValidator>();
builder.Services.AddScoped<IVisitService, VisitService>();

// Auth
var useDev = builder.Configuration.GetValue<bool>("Auth:UseDevAuth");

if (useDev)
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = DevAuthHandler.Scheme;
        options.DefaultChallengeScheme    = DevAuthHandler.Scheme;
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
                ValidateIssuer = !string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Authority"]),
                ValidIssuer = builder.Configuration["Jwt:Authority"],
                ValidateAudience = !string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Audience"]),
                ValidAudience = builder.Configuration["Jwt:Audience"],
                ValidateIssuerSigningKey = !string.IsNullOrWhiteSpace(builder.Configuration["Jwt:SigningKey"]),
                IssuerSigningKey = string.IsNullOrWhiteSpace(builder.Configuration["Jwt:SigningKey"])
                    ? null
                    : new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!)),
                ValidateLifetime = true
            };
        });
}

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("VisitsRead", policy =>
        policy.RequireAssertion(ctx => Policies.HasScopeOrRole(ctx.User, Policies.ReadScope)));
    options.AddPolicy("VisitsWrite", policy =>
        policy.RequireAssertion(ctx => Policies.HasScopeOrRole(ctx.User, Policies.WriteScope)));
});

// Controllers + ProblemDetails + Swagger
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = null; // keep case for TruckLicencePlate & DriverInformation
});
builder.Services.AddProblemDetails(); // used by our exception mapping
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

// Rate limiting (simple fixed-window)
var perMinute = builder.Configuration.GetValue("RateLimiting:PermitPerMinute", 120);
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = perMinute;
        options.Window = TimeSpan.FromMinutes(1);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = perMinute; // allow queue equal to limit
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

app.MapHealthChecks("/health/live");

app.MapControllers();

app.Run();

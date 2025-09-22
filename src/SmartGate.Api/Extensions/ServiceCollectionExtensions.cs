using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmartGate.Api.Auth;
using SmartGate.Api.Common;
using SmartGate.Api.ErrorHandling;
using SmartGate.Application.Abstractions;
using SmartGate.Application.Visits;
using SmartGate.Application.Visits.Dto;
using SmartGate.Application.Visits.Ports;
using SmartGate.Application.Visits.Validators;
using SmartGate.Infrastructure.Database;
using SmartGate.Infrastructure.Repositories;

namespace SmartGate.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddDbContext<SmartGateDbContext>(o =>
        {
            var cs = configuration.GetConnectionString("Postgres");
            var isDev = environment.IsDevelopment();
            o.UseNpgsql(cs)
             .EnableDetailedErrors(isDev)
             .EnableSensitiveDataLogging(isDev);
        });
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddScoped<IUserContext, HttpUserContext>();
        services.AddScoped<IClock, SystemClock>();
        services.AddScoped<IPiiPolicy, PassthroughPiiPolicy>();
        services.AddScoped<IVisitRepository, VisitRepository>();
        services.AddScoped<IIdempotencyStore, IdempotencyStore>();
        services.AddScoped<IValidator<CreateVisitRequest>, CreateVisitRequestValidator>();
        services.AddScoped<IVisitService, VisitService>();
        services.AddScoped<IDriverRepository, DriverRepository>();
        services.AddSingleton<ProblemDetailsFactory, FlatErrorsProblemDetailsFactory>();
        return services;
    }

    public static IServiceCollection AddSmartGateAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var useDev = configuration.GetValue<bool>("Auth:UseDevAuth");
        
        if (useDev)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = DevAuthHandler.Scheme;
                options.DefaultChallengeScheme = DevAuthHandler.Scheme;
            }).AddScheme<DevAuthOptions, DevAuthHandler>(DevAuthHandler.Scheme, _ => { });
        }
        else
        {
            var jwtAuthority = configuration["Jwt:Authority"];
            var jwtSigningKey = configuration["Jwt:SigningKey"];
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/problem+json";
                            var problem = new
                            {
                                type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                                title = "Unauthorized",
                                status = 401,
                                detail = "Authentication required."
                            };
                            return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(problem));
                        }
                    };
                });
        }

        var authBuilder = services.AddAuthorizationBuilder()
            .AddPolicy("VisitsRead", policy =>
                policy.RequireAssertion(ctx => Policies.HasScopeOrRole(ctx.User, Policies.ReadScope)))
            .AddPolicy("VisitsWrite", policy =>
                policy.RequireAssertion(ctx => Policies.HasScopeOrRole(ctx.User, Policies.WriteScope)));

        if (!useDev)
        {
            authBuilder.AddFallbackPolicy("RequireAuth", policy => policy.RequireAuthenticatedUser());
        }

        return services;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            o.JsonSerializerOptions.Converters.Add(new StringTryParseConverterFactory());
            o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
            o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });
        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();
        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {

        
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(AppConstants.ApiVersion, new OpenApiInfo { Title = AppConstants.ServiceName, Version = AppConstants.ApiVersion });
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
        return services;
    }

    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var perMinute = configuration.GetValue("RateLimiting:PermitPerMinute", 120);
        services.AddRateLimiter(_ => _
            .AddFixedWindowLimiter("fixed", options =>
            {
                options.PermitLimit = perMinute;
                options.Window = TimeSpan.FromMinutes(1);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = perMinute;
            }));
        return services;
    }
}
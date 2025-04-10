using FloByte.Application.Common.Interfaces;
using FloByte.Infrastructure.Identity;
using FloByte.Infrastructure.Persistence;
using FloByte.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

namespace FloByte.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        // Identity
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Services
        services.AddScoped<IDateTime, DateTimeService>();
        services.AddScoped<IDatabaseManagementService, DatabaseManagementService>();
        services.AddScoped<IWorkflowEngine, WorkflowEngineService>();
        services.AddScoped<ICodeEditorService, CodeEditorService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<ILoggingService, LoggingService>();
        services.AddScoped<IBlobStorageService, BlobStorageService>();

        // Background Services
        services.AddHostedService<WorkflowExecutionService>();
        services.AddHostedService<CodeIndexingService>();

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var options = ConfigurationOptions.Parse(
                configuration.GetConnectionString("Redis") ?? "localhost:6379");
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });

        // Caching
        services.AddMemoryCache();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "FloByte:";
        });

        // Logging
        services.AddApplicationInsightsTelemetry();
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .WriteTo.Console()
            .WriteTo.ApplicationInsights(
                configuration["ApplicationInsights:ConnectionString"],
                new TraceTelemetryConverter())
            .CreateLogger();

        services.AddLogging(builder => builder.AddSerilog(dispose: true));

        // OpenTelemetry
        services.AddOpenTelemetry()
            .WithTracing(builder => builder
                .AddSource("FloByte.API")
                .SetResourceBuilder(ResourceBuilder
                    .CreateDefault()
                    .AddService("FloByte.API"))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSqlClientInstrumentation()
                .AddRedisInstrumentation()
                .AddOtlpExporter())
            .WithMetrics(builder => builder
                .AddMeter("FloByte.API")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter());

        return services;
    }

    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var oidcSettings = configuration.GetSection(nameof(OidcSettings)).Get<OidcSettings>();
        services.Configure<OidcSettings>(configuration.GetSection(nameof(OidcSettings)));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = oidcSettings?.Authority;
                options.Audience = oidcSettings?.Audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                    {
                        var identityService = context.HttpContext.RequestServices
                            .GetRequiredService<IIdentityService>();

                        var claims = new OidcClaims
                        {
                            Subject = context.Principal?.FindFirst("sub")?.Value,
                            Name = context.Principal?.FindFirst("name")?.Value,
                            Email = context.Principal?.FindFirst("email")?.Value,
                            Picture = context.Principal?.FindFirst("picture")?.Value
                        };

                        await identityService.GetOrCreateUserAsync(claims);
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdminRole", policy =>
                policy.RequireRole("Admin"));

            options.AddPolicy("RequireProjectAccess", policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim(c => c.Type == "ProjectAccess")));

            options.AddPolicy("RequireWorkflowAccess", policy =>
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim(c => c.Type == "WorkflowAccess")));
        });

        return services;
    }
}

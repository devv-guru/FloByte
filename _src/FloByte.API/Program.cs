using FloByte.Application;
using FloByte.Application.Features.Users.Commands;
using FloByte.Application.Features.Users.Queries;
using FloByte.Application.Features.Projects.Commands;
using FloByte.Application.Features.Projects.Queries;
using FloByte.Application.Features.Workflows.Commands;
using FloByte.Application.Features.Workflows.Queries;
using FloByte.Application.Features.CodeEditor.Commands;
using FloByte.Application.Features.CodeEditor.Queries;
using FloByte.Infrastructure;
using FloByte.API.Filters;
using FloByte.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.AspNetCore.Mvc.Versioning;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
{
    module.EnableSqlCommandTextInstrumentation = true;
});

// Add OpenTelemetry
var meter = new Meter("FloByte.API");
var requestCounter = meter.CreateCounter<long>("api.requests.total");
var activeRequests = meter.CreateUpDownCounter<long>("api.requests.active");

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
        metrics.AddMeter("FloByte.API")
            .AddRuntimeInstrumentation()
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation())
    .WithTracing(tracing =>
        tracing.AddSource("FloByte.API")
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddEntityFrameworkCoreInstrumentation());

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("x-api-version"),
        new MediaTypeApiVersionReader("v"));
});

// Add services to the container
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddUrlGroup(new Uri(builder.Configuration["OidcSettings:Authority"]!), name: "identity-server");

// Add response caching and rate limiting
builder.Services.AddResponseCaching();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "FloByte:";
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Too many requests. Please try again later.",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) ? retryAfter.TotalSeconds : null
        }, token);
    };
});

// Configure authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Authentication:Authority"];
    options.Audience = builder.Configuration["Authentication:Audience"];
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero
    };
})
.AddOpenIdConnect(options =>
{
    options.Authority = builder.Configuration["Authentication:Authority"];
    options.ClientId = builder.Configuration["Authentication:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:ClientSecret"];
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
});

// Configure authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
});

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "FloByte API",
        Version = "v1",
        Description = @"FloByte API provides endpoints for managing users, projects, workflows, and code files.
            
Features:
- User management with OIDC authentication
- Project collaboration
- Visual workflow designer
- Code editor with version control
- Real-time collaboration
            
For more information, visit our [documentation](https://docs.flobyte.dev).",
        Contact = new()
        {
            Name = "FloByte Support",
            Email = "support@flobyte.dev",
            Url = new Uri("https://flobyte.dev/support")
        },
        License = new()
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    options.AddSecurityDefinition("oauth2", new()
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new()
        {
            AuthorizationCode = new()
            {
                AuthorizationUrl = new Uri(builder.Configuration["OidcSettings:Authority"] + "/connect/authorize"),
                TokenUrl = new Uri(builder.Configuration["OidcSettings:Authority"] + "/connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "api", "API Access" }
                }
            }
        }
    });

    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "FloByte.API.xml"));
    options.EnableAnnotations();
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId(builder.Configuration["OidcSettings:ClientId"]);
        options.OAuthUsePkce();
        options.EnableDeepLinking();
        options.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowedOrigins");
app.UseResponseCaching();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Add request telemetry
app.Use(async (context, next) =>
{
    requestCounter.Add(1);
    activeRequests.Add(1);
    
    try
    {
        await next(context);
    }
    finally
    {
        activeRequests.Add(-1);
    }
});

// User endpoints
var users = app.MapGroup("/api/v{version:apiVersion}/users")
    .WithTags("Users")
    .RequireAuthorization()
    .WithOpenApi();

users.MapGet("/", async (
    ISender sender,
    CancellationToken ct) =>
{
    var result = await sender.Send(new GetUsers(), ct);
    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.BadRequest(result.Errors);
})
.WithName("GetUsers")
.CacheOutput(x => x.Expire(TimeSpan.FromMinutes(5)));

users.MapGet("/{userId}", async (
    Guid userId,
    ISender sender,
    CancellationToken ct) =>
{
    var result = await sender.Send(new GetUserById(userId), ct);
    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.NotFound();
})
.WithName("GetUserById")
.CacheOutput(x => x.Expire(TimeSpan.FromMinutes(5)));

users.MapPost("/", async (
    OidcClaims claims,
    ISender sender,
    CancellationToken ct) =>
{
    var result = await sender.Send(new CreateUser(claims), ct);
    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.BadRequest(result.Errors);
})
.WithName("CreateUser")
.AddEndpointFilter<ValidationFilter<CreateUser>>();

users.MapPut("/{userId}/profile", async (
    Guid userId,
    UpdateUserProfile command,
    ISender sender,
    CancellationToken ct) =>
{
    command = command with { UserId = userId };
    var result = await sender.Send(command, ct);
    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.BadRequest(result.Errors);
})
.WithName("UpdateUserProfile")
.AddEndpointFilter<ValidationFilter<UpdateUserProfile>>();

// Project endpoints
var projects = app.MapGroup("/api/v{version:apiVersion}/projects")
    .WithTags("Projects")
    .RequireAuthorization()
    .WithOpenApi();

projects.MapGet("/", async (
    ISender sender,
    CancellationToken ct) =>
{
    var result = await sender.Send(new GetUserProjects(), ct);
    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.BadRequest(result.Errors);
})
.WithName("GetUserProjects")
.CacheOutput(x => x.Expire(TimeSpan.FromMinutes(5)));

projects.MapGet("/{projectId}", async (
    Guid projectId,
    ISender sender,
    CancellationToken ct) =>
{
    var result = await sender.Send(new GetProjectDetails(projectId), ct);
    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.NotFound();
})
.WithName("GetProjectDetails")
.AddEndpointFilter<ProjectAccessFilter>()
.CacheOutput(x => x.Expire(TimeSpan.FromMinutes(5)));

projects.MapPost("/", async (
    CreateProject command,
    ISender sender,
    CancellationToken ct) =>
{
    var result = await sender.Send(command, ct);
    return result.IsSuccess
        ? Results.Created($"/api/projects/{result.Value.Id}", result.Value)
        : Results.BadRequest(result.Errors);
})
.WithName("CreateProject")
.AddEndpointFilter<ValidationFilter<CreateProject>>();

projects.MapPost("/{projectId}/members", async (
    Guid projectId,
    Guid userId,
    ISender sender,
    CancellationToken ct) =>
{
    var command = new AddProjectMember(projectId, userId);
    var result = await sender.Send(command, ct);
    return result.IsSuccess
        ? Results.NoContent()
        : Results.BadRequest(result.Errors);
})
.WithName("AddProjectMember")
.AddEndpointFilter<ProjectAccessFilter>();

// Workflow endpoints
var workflows = app.MapGroup("/api/v{version:apiVersion}/workflows")
    .WithTags("Workflows")
    .RequireAuthorization()
    .WithOpenApi();

workflows.MapGet("/{workflowId}", async (
    Guid workflowId,
    ISender sender,
    CancellationToken ct) =>
{
    var result = await sender.Send(new GetWorkflowGraph(workflowId), ct);
    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.NotFound();
})
.WithName("GetWorkflowGraph")
.CacheOutput(x => x.Expire(TimeSpan.FromMinutes(1)));

workflows.MapPost("/", async (
    CreateWorkflow command,
    ISender sender,
    CancellationToken ct) =>
{
    var result = await sender.Send(command, ct);
    return result.IsSuccess
        ? Results.Created($"/api/workflows/{result.Value.Id}", result.Value)
        : Results.BadRequest(result.Errors);
})
.WithName("CreateWorkflow")
.AddEndpointFilter<ValidationFilter<CreateWorkflow>>();

workflows.MapPost("/{workflowId}/nodes", async (
    Guid workflowId,
    AddWorkflowNode command,
    ISender sender,
    CancellationToken ct) =>
{
    command = command with { WorkflowId = workflowId };
    var result = await sender.Send(command, ct);
    return result.IsSuccess
        ? Results.Created($"/api/workflows/{workflowId}/nodes/{result.Value.Id}", result.Value)
        : Results.BadRequest(result.Errors);
})
.WithName("AddWorkflowNode")
.AddEndpointFilter<ValidationFilter<AddWorkflowNode>>();

workflows.MapPost("/{workflowId}/connections", async (
    Guid workflowId,
    ConnectWorkflowNodes command,
    ISender sender,
    CancellationToken ct) =>
{
    command = command with { WorkflowId = workflowId };
    var result = await sender.Send(command, ct);
    return result.IsSuccess
        ? Results.Created($"/api/workflows/{workflowId}/connections/{result.Value.Id}", result.Value)
        : Results.BadRequest(result.Errors);
})
.WithName("ConnectWorkflowNodes")
.AddEndpointFilter<ValidationFilter<ConnectWorkflowNodes>>();

// Code editor endpoints
var codeEditor = app.MapGroup("/api/v{version:apiVersion}/code")
    .WithTags("Code Editor")
    .RequireAuthorization()
    .WithOpenApi();

codeEditor.MapGet("/files/{fileId}", async (
    Guid fileId,
    ISender sender,
    CancellationToken ct) =>
{
    var result = await sender.Send(new GetFileHistory(fileId), ct);
    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.NotFound();
})
.WithName("GetFileHistory")
.CacheOutput(x => x.Expire(TimeSpan.FromMinutes(5)));

codeEditor.MapPost("/files", async (
    CreateCodeFile command,
    ISender sender,
    CancellationToken ct) =>
{
    var result = await sender.Send(command, ct);
    return result.IsSuccess
        ? Results.Created($"/api/code/files/{result.Value.Id}", result.Value)
        : Results.BadRequest(result.Errors);
})
.WithName("CreateCodeFile")
.AddEndpointFilter<ValidationFilter<CreateCodeFile>>();

codeEditor.MapPut("/files/{fileId}", async (
    Guid fileId,
    SaveCodeFile command,
    ISender sender,
    CancellationToken ct) =>
{
    command = command with { FileId = fileId };
    var result = await sender.Send(command, ct);
    return result.IsSuccess
        ? Results.Ok(result.Value)
        : Results.BadRequest(result.Errors);
})
.WithName("SaveCodeFile")
.AddEndpointFilter<ValidationFilter<SaveCodeFile>>();

codeEditor.MapPost("/files/{fileId}/comments", async (
    Guid fileId,
    AddCodeComment command,
    ISender sender,
    CancellationToken ct) =>
{
    command = command with { FileId = fileId };
    var result = await sender.Send(command, ct);
    return result.IsSuccess
        ? Results.Created($"/api/code/files/{fileId}/comments/{result.Value.Id}", result.Value)
        : Results.BadRequest(result.Errors);
})
.WithName("AddCodeComment")
.AddEndpointFilter<ValidationFilter<AddCodeComment>>();

// Search endpoints
var search = app.MapGroup("/api/v{version:apiVersion}/search")
    .WithTags("Search")
    .RequireAuthorization()
    .WithOpenApi();

search.MapGet("/projects", async (
    string? query,
    [AsParameters] SearchOptions options,
    ISender sender,
    CancellationToken ct) =>
{
    var result = await sender.Send(new SearchProjects(query, options), ct);
    return Results.Ok(new
    {
        items = result.Value,
        total = result.Value.Count,
        page = options.Page,
        pageSize = options.PageSize
    });
})
.WithName("SearchProjects")
.CacheOutput(x => x.Expire(TimeSpan.FromMinutes(1)));

search.MapGet("/code", async (
    string? query,
    string? language,
    [AsParameters] SearchOptions options,
    ISender sender,
    CancellationToken ct) =>
{
    var result = await sender.Send(new SearchCodeFiles(query, language, options), ct);
    return Results.Ok(new
    {
        items = result.Value,
        total = result.Value.Count,
        page = options.Page,
        pageSize = options.PageSize
    });
})
.WithName("SearchCode")
.CacheOutput(x => x.Expire(TimeSpan.FromMinutes(1)));

// Batch operations
var batch = app.MapGroup("/api/v{version:apiVersion}/batch")
    .WithTags("Batch Operations")
    .RequireAuthorization()
    .WithOpenApi();

batch.MapPost("/workflows", async (
    BatchUpdateWorkflows command,
    ISender sender,
    CancellationToken ct) =>
{
    var result = await sender.Send(command, ct);
    return Results.Ok(result.Value);
})
.WithName("BatchUpdateWorkflows")
.AddEndpointFilter<ValidationFilter<BatchUpdateWorkflows>>();

// Health checks with detailed status
app.MapHealthChecks("/health", new()
{
    ResponseWriter = UIHealthChecksResponseWriter.WriteDetailedResponse
});

// Metrics endpoint for Prometheus
app.MapPrometheusScrapingEndpoint();

app.Run();

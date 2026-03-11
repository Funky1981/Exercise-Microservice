using Asp.Versioning;
using Exercise.API;
using Exercise.API.Middleware;
using Exercise.API.Services;
using Exercise.Application;
using Exercise.Application.Abstractions.Services;
using Exercise.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// 🟧 Serilog — reads from appsettings.json; AddSerilog does not freeze the static
// Log.Logger so WebApplicationFactory test factories can run cleanly.
builder.Services.AddSerilog(lc => lc
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext());

// IHttpContextAccessor — used by CorrelationIdMiddleware and any service that
// needs access to the current HttpContext outside of a controller/endpoint.
builder.Services.AddHttpContextAccessor();

// 🟧 Application layer (MediatR, AutoMapper, FluentValidation) ??????????????
builder.Services.AddApplication();

// ?? Infrastructure layer (EF Core + ExerciseRepository) ???????????????????
builder.Services.AddInfrastructure(builder.Configuration);

// ?? JWT Token Service ??????????????????????????????????????????????????????????
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

// HttpClient for RapidAPI is now registered in Infrastructure DI (AddInfrastructure).

// ?? JWT Bearer Authentication ??????????????????????????????????????????????
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key 'Jwt:Key' is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    // "Admin" policy — only users with Role=Admin can call admin endpoints
    // (exercise catalogue mutations, sync, etc.).
    options.AddPolicy("Admin", policy =>
        policy.RequireRole("Admin"));
});

// ?? API Versioning ???????????????????????????????????????????????????????????
// Header-based (api-version: 1.0) + query-string (?api-version=1.0).
// Routes are unchanged — existing clients need no modification.
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion                   = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions                   = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new HeaderApiVersionReader("api-version"),
        new QueryStringApiVersionReader("api-version"));
});

// ?? Rate Limiting (built-in .NET 7+, no NuGet required) ????????????????
// "auth" policy: 10 req/60 s per IP on auth routes to slow brute-force
// "api"  policy: 30 req/60 s per IP on all other routes
var authPermitLimit = builder.Configuration.GetValue("RateLimiting:Auth:PermitLimit", 10);
var authWindowSeconds = builder.Configuration.GetValue("RateLimiting:Auth:WindowSeconds", 60);
var apiPermitLimit = builder.Configuration.GetValue("RateLimiting:Api:PermitLimit", 30);
var apiWindowSeconds = builder.Configuration.GetValue("RateLimiting:Api:WindowSeconds", 60);
var rateLimitingEnabled = builder.Configuration.GetValue("RateLimiting:Enabled", true);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}:{httpContext.Request.Path.Value?.ToLowerInvariant() ?? "/"}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit          = authPermitLimit,
                Window               = TimeSpan.FromSeconds(authWindowSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit           = 0
            }));

    options.AddPolicy("api", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit          = apiPermitLimit,
                Window               = TimeSpan.FromSeconds(apiWindowSeconds),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit           = 0
            }));
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ExerciseDbContext>("database");

// 🟧 Output Cache — exercise catalogue changes rarely; cache per page params.
// Tag "exercises" lets us evict the cache on POST/PUT/DELETE via IOutputCacheStore.
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("ExerciseCatalogue", policy =>
        policy
            .Expire(TimeSpan.FromMinutes(5))
            .SetVaryByQuery("pageNumber", "pageSize")
            .Tag("exercises"));
});

// ?? OpenAPI / Swagger ??????????????????????????????????????????????????????
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "Exercise Microservice API",
        Version = "v1",
        Description = "A .NET 9 microservice for fitness tracking and exercise management."
    });

    // Allow JWT tokens to be entered in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token below."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ⚠️ Migrations are NOT auto-applied on startup to avoid race conditions
// with multiple replicas. Run migrations externally:
//   dotnet ef database update --project Exercise.Infrastructure --startup-project Exercise.API
// Or use a Kubernetes init container / CI pipeline step.

// ?? HTTP pipeline ??????????????????????????????????????????????????????????
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Exercise Microservice API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Security response headers — applied to every response
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");
    context.Response.Headers.Append("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
    if (!app.Environment.IsDevelopment())
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    await next();
});

// Correlation ID — must run before Serilog request logging so the CorrelationId
// property is in LogContext when UseSerilogRequestLogging fires.
app.UseMiddleware<CorrelationIdMiddleware>();

// Structured HTTP request logging via Serilog (before auth/endpoint middleware)
app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();
if (rateLimitingEnabled)
{
    app.UseRateLimiter();
}

// ?? Minimal API endpoints ??????????????????????????????????????????????????
app.MapAuthEndpointsRoute();
app.MapExerciseEndpoints();
app.MapUserEndpointsRoute();
app.MapWorkoutEndpointsRoute();
app.MapWorkoutPlanEndpointsRoute();
app.MapExerciseLogEndpointsRoute();
app.MapRapidApiEndpoints();
app.MapAnalyticsEndpointsRoute();

// ?? Health checks ??????????????????????????????????????????????????????????
// GET /health          — simple liveness probe (200/503)
// GET /health/detail   — detailed JSON with per-check duration and status
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/detail", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            status   = report.Status.ToString(),
            duration = report.TotalDuration.TotalMilliseconds,
            checks   = report.Entries.Select(e => new
            {
                name     = e.Key,
                status   = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                error    = e.Value.Exception?.Message
            })
        });
    }
});

app.Run();

// Required for integration tests (WebApplicationFactory<Program>)
public partial class Program { }


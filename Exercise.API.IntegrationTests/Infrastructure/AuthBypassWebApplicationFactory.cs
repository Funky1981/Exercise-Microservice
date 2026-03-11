using Exercise.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Exercise.API.IntegrationTests.Infrastructure;

/// <summary>
/// A variant factory that bypasses JWT authentication, replacing it with a simple
/// test handler that always authenticates. Use this for testing business logic
/// on authenticated endpoints without needing real JWT tokens.
/// </summary>
public class AuthBypassWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public Guid AuthenticatedUserId
    {
        get  => TestAuthHandler.UserId;
        set  => TestAuthHandler.UserId = value;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "DataSource=:memory:",
                ["RapidApi:Host"]                       = "placeholder.example.com",
                ["RapidApi:Key"]                        = "placeholder",
                ["Serilog:MinimumLevel:Default"]         = "Error",
                ["RateLimiting:Auth:PermitLimit"]        = "1000",
                ["RateLimiting:Api:PermitLimit"]         = "1000",
                ["RateLimiting:Enabled"]                 = "false",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Swap SQL Server DbContext for SQLite.
            var toRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ExerciseDbContext>)
                         || d.ServiceType == typeof(DbContextOptions))
                .ToList();
            foreach (var d in toRemove)
                services.Remove(d);

            var sqliteOptions = new DbContextOptionsBuilder<ExerciseDbContext>()
                .UseSqlite(_connection)
                .Options;
            services.AddSingleton(sqliteOptions);
            services.AddSingleton<DbContextOptions>(sqliteOptions);

            // Remove all existing authentication registrations so we can replace
            // the default scheme with our always-authenticated test handler.
            var authDescriptors = services
                .Where(d => d.ServiceType == typeof(IAuthenticationSchemeProvider)
                         || d.ServiceType == typeof(IAuthenticationHandlerProvider)
                         || d.ServiceType == typeof(IAuthenticationService)
                         || d.ServiceType == typeof(IClaimsTransformation)
                         || d.ServiceType == typeof(IOptionsMonitor<AuthenticationOptions>)
                         || d.ServiceType == typeof(IConfigureOptions<AuthenticationOptions>)
                         || d.ServiceType == typeof(IPostConfigureOptions<AuthenticationOptions>))
                .ToList();
            foreach (var d in authDescriptors)
                services.Remove(d);

            // Register our test scheme as the default (and only) scheme.
            services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

            // Replace all authorization policies with a simple requirement that any
            // authenticated user passes — this matches RequireAuthorization() behaviour.
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder("Test")
                    .RequireAuthenticatedUser()
                    .Build();

                // Mirror the Admin policy from production but use the "Test" scheme
                options.AddPolicy("Admin", policy =>
                    policy.AddAuthenticationSchemes("Test")
                          .RequireRole("Admin"));

                options.FallbackPolicy = null;
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ExerciseDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _connection.CloseAsync();
        await base.DisposeAsync();
    }
}

using Exercise.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Exercise.API.IntegrationTests.Infrastructure;

/// <summary>
/// Shared test server that replaces SQL Server with an in-memory SQLite database
/// so integration tests run without any external dependencies.
/// </summary>
public class ExerciseWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // A single open connection keeps the :memory: database alive for the lifetime of the factory.
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Inject test-safe configuration values that override user-secrets / appsettings.
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                // Point DB connection to in-memory SQLite (overridden by DbContext factory anyway)
                ["ConnectionStrings:DefaultConnection"] = "DataSource=:memory:",
                // RapidAPI key is not needed for integration tests
                ["RapidApi:Host"] = "placeholder.example.com",
                ["RapidApi:Key"]  = "placeholder",
                // Logging
                ["Logging:LogLevel:Default"]                        = "Warning",
                ["Logging:LogLevel:Microsoft.EntityFrameworkCore"]  = "Warning",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the registered DbContextOptions<ExerciseDbContext> that contains
            // the SqlServer extension (UseSqlServer) added by AddInfrastructure.
            var toRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ExerciseDbContext>)
                         || d.ServiceType == typeof(DbContextOptions))
                .ToList();

            foreach (var d in toRemove)
                services.Remove(d);

            // Build a clean Sqlite options instance without any SqlServer extensions.
            var sqliteOptions = new DbContextOptionsBuilder<ExerciseDbContext>()
                .UseSqlite(_connection)
                .Options;

            // Register as singleton so all scopes share the same options & connection.
            services.AddSingleton(sqliteOptions);
            services.AddSingleton<DbContextOptions>(sqliteOptions);

            // Override JWT bearer to use the same key that ITokenService will use at runtime.
            // PostConfigure runs after all AddAuthentication/AddJwtBearer calls so it wins.
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, opts =>
            {
                // Resolve the effective Jwt:Key from the test container's IConfiguration.
                using var sp = services.BuildServiceProvider();
                var cfg    = sp.GetRequiredService<IConfiguration>();
                var key    = cfg["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured.");
                var issuer = cfg["Jwt:Issuer"] ?? "ExerciseMicroservice";
                var aud    = cfg["Jwt:Audience"] ?? "ExerciseMicroserviceUsers";

                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = issuer,
                    ValidAudience            = aud,
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };

                // Log auth failures to test output
                opts.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        Console.Error.WriteLine($"[JWT Auth Failure] {ctx.Exception.GetType().Name}: {ctx.Exception.Message}");
                        return Task.CompletedTask;
                    }
                };
            });
        });
    }

    /// <summary>Called by xUnit before any tests in the fixture run.</summary>
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

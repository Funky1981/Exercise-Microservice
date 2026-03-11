using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Exercise.API.IntegrationTests.Infrastructure;

/// <summary>
/// A test authentication handler that authenticates every request as a pre-configured user.
/// Used in integration tests to avoid real JWT signing/validation complexity.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>The user ID that will be set in the sub/nameidentifier claim.</summary>
    public static Guid UserId { get; set; } = Guid.NewGuid();

    /// <summary>The role that will be set in the role claim (default: Admin for backward compatibility with existing tests).</summary>
    public static string UserRole { get; set; } = "Admin";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, UserId.ToString()),
            new Claim(ClaimTypes.Email,          $"testuser_{UserId:N}@test.com"),
            new Claim(ClaimTypes.Name,           "Test User"),
            new Claim(ClaimTypes.Role,           UserRole),
        };

        var identity  = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket    = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

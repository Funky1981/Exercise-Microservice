using System.Net;
using System.Net.Http.Json;
using Exercise.API.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Exercise.API.IntegrationTests.Tests.Users;

/// <summary>
/// Integration tests for the /api/users endpoints.
/// Ownership guard tests verify that a user cannot access another user's profile.
/// </summary>
public class UserEndpointTests : IClassFixture<ExerciseWebApplicationFactory>,
                                 IClassFixture<AuthBypassWebApplicationFactory>
{
    private readonly ExerciseWebApplicationFactory   _realFactory;
    private readonly AuthBypassWebApplicationFactory _bypassFactory;

    public UserEndpointTests(
        ExerciseWebApplicationFactory   realFactory,
        AuthBypassWebApplicationFactory bypassFactory)
    {
        _realFactory   = realFactory;
        _bypassFactory = bypassFactory;
    }

    [Fact]
    public async Task GetUser_WithoutToken_ReturnsUnauthorized()
    {
        var client   = _realFactory.CreateClient();
        var response = await client.GetAsync($"/api/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Ownership guard (403) ────────────────────────────────────────────────

    [Fact]
    public async Task GetUser_ByDifferentUser_ReturnsForbidden()
    {
        var original = _bypassFactory.AuthenticatedUserId;
        try
        {
            // JWT sub belongs to user A
            var userAId = Guid.NewGuid();
            _bypassFactory.AuthenticatedUserId = userAId;

            var client = _bypassFactory.CreateClient();

            // Request user B's profile while authenticated as user A → 403
            var userBId  = Guid.NewGuid();
            var response = await client.GetAsync($"/api/users/{userBId}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        finally { _bypassFactory.AuthenticatedUserId = original; }
    }

    [Fact]
    public async Task UpdateUserProfile_ByDifferentUser_ReturnsForbidden()
    {
        var original = _bypassFactory.AuthenticatedUserId;
        try
        {
            var userAId = Guid.NewGuid();
            _bypassFactory.AuthenticatedUserId = userAId;

            var client  = _bypassFactory.CreateClient();
            var userBId = Guid.NewGuid();

            var response = await client.PutAsJsonAsync($"/api/users/{userBId}/profile", new
            {
                username = "hacker",
                height   = 180,
                weight   = 80
            });

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        finally { _bypassFactory.AuthenticatedUserId = original; }
    }

    [Fact]
    public async Task DeleteUser_ByDifferentUser_ReturnsForbidden()
    {
        var original = _bypassFactory.AuthenticatedUserId;
        try
        {
            var userAId = Guid.NewGuid();
            _bypassFactory.AuthenticatedUserId = userAId;

            var client  = _bypassFactory.CreateClient();
            var userBId = Guid.NewGuid();

            var response = await client.DeleteAsync($"/api/users/{userBId}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        finally { _bypassFactory.AuthenticatedUserId = original; }
    }
}

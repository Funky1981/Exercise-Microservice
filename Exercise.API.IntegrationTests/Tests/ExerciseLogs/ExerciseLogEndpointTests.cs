using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Exercise.API.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Exercise.API.IntegrationTests.Tests.ExerciseLogs;

/// <summary>
/// Integration tests for the /api/exercise-logs endpoints.
/// Unauthorized tests use the real JWT factory; authorized tests bypass JWT via TestAuthHandler.
/// </summary>
[Collection("Integration")]
public class ExerciseLogEndpointTests : DualFactoryIntegrationTestBase
{
    public ExerciseLogEndpointTests(
        ExerciseWebApplicationFactory   realFactory,
        AuthBypassWebApplicationFactory bypassFactory)
        : base(realFactory, bypassFactory) { }

    [Fact]
    public async Task GetExerciseLogs_WithoutToken_ReturnsUnauthorized()
    {
        var client   = RealFactory.CreateClient();
        var response = await client.GetAsync("/api/exercise-logs?pageNumber=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetExerciseLogs_WithValidToken_ReturnsOk()
    {
        var client   = BypassFactory.CreateClient();
        var response = await client.GetAsync("/api/exercise-logs?pageNumber=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateExerciseLog_WithValidToken_Returns201()
    {
        var client = BypassFactory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/exercise-logs", new
        {
            name  = "Morning Session",
            date  = DateTime.UtcNow,
            notes = "Integration test log"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateExerciseLog_ThenGetExerciseLogs_ReturnsThatLog()
    {
        var client = BypassFactory.CreateClient();

        await client.PostAsJsonAsync("/api/exercise-logs", new
        {
            name  = "Evening Stretch",
            date  = DateTime.UtcNow,
            notes = "Flexibility work"
        });

        var listResp = await client.GetAsync("/api/exercise-logs?pageNumber=1&pageSize=20");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await listResp.Content.ReadAsStringAsync();
        body.Should().Contain("Evening Stretch");
    }

    // ── Ownership guard (403) ────────────────────────────────────────────────

    private async Task<Guid> CreateExerciseLogAsCurrentUserAsync(HttpClient client)
    {
        var resp = await client.PostAsJsonAsync("/api/exercise-logs", new
        {
            name  = "Ownership Test Log",
            date  = DateTime.UtcNow,
            notes = "ownership guard"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task GetExerciseLogById_ByDifferentUser_ReturnsForbidden()
    {
        var original = BypassFactory.AuthenticatedUserId;
        try
        {
            BypassFactory.AuthenticatedUserId = Guid.NewGuid();
            var client = BypassFactory.CreateClient();
            var logId = await CreateExerciseLogAsCurrentUserAsync(client);

            BypassFactory.AuthenticatedUserId = Guid.NewGuid();
            var response = await client.GetAsync($"/api/exercise-logs/{logId}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        finally { BypassFactory.AuthenticatedUserId = original; }
    }

    [Fact]
    public async Task DeleteExerciseLog_ByDifferentUser_ReturnsForbidden()
    {
        var original = BypassFactory.AuthenticatedUserId;
        try
        {
            BypassFactory.AuthenticatedUserId = Guid.NewGuid();
            var client = BypassFactory.CreateClient();
            var logId = await CreateExerciseLogAsCurrentUserAsync(client);

            BypassFactory.AuthenticatedUserId = Guid.NewGuid();
            var response = await client.DeleteAsync($"/api/exercise-logs/{logId}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        finally { BypassFactory.AuthenticatedUserId = original; }
    }
}

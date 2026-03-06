using System.Net;
using System.Net.Http.Json;
using Exercise.API.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Exercise.API.IntegrationTests.Tests.ExerciseLogs;

/// <summary>
/// Integration tests for the /api/exercise-logs endpoints.
/// Unauthorized tests use the real JWT factory; authorized tests bypass JWT via TestAuthHandler.
/// </summary>
public class ExerciseLogEndpointTests : IClassFixture<ExerciseWebApplicationFactory>,
                                        IClassFixture<AuthBypassWebApplicationFactory>
{
    private readonly ExerciseWebApplicationFactory   _realFactory;
    private readonly AuthBypassWebApplicationFactory _bypassFactory;

    public ExerciseLogEndpointTests(
        ExerciseWebApplicationFactory   realFactory,
        AuthBypassWebApplicationFactory bypassFactory)
    {
        _realFactory   = realFactory;
        _bypassFactory = bypassFactory;
    }

    [Fact]
    public async Task GetExerciseLogs_WithoutToken_ReturnsUnauthorized()
    {
        var client   = _realFactory.CreateClient();
        var response = await client.GetAsync("/api/exercise-logs?pageNumber=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetExerciseLogs_WithValidToken_ReturnsOk()
    {
        var client   = _bypassFactory.CreateClient();
        var response = await client.GetAsync("/api/exercise-logs?pageNumber=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateExerciseLog_WithValidToken_Returns201()
    {
        var client = _bypassFactory.CreateClient();

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
        var client = _bypassFactory.CreateClient();

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
}

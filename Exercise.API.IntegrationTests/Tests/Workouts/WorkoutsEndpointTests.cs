using System.Net;
using System.Net.Http.Json;
using Exercise.API.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Exercise.API.IntegrationTests.Tests.Workouts;

/// <summary>
/// Integration tests for the /api/workouts endpoints.
/// Unauthorized tests use the real JWT factory; authorized tests bypass JWT via TestAuthHandler.
/// </summary>
public class WorkoutsEndpointTests : IClassFixture<ExerciseWebApplicationFactory>,
                                     IClassFixture<AuthBypassWebApplicationFactory>
{
    private readonly ExerciseWebApplicationFactory   _realFactory;
    private readonly AuthBypassWebApplicationFactory _bypassFactory;

    public WorkoutsEndpointTests(
        ExerciseWebApplicationFactory   realFactory,
        AuthBypassWebApplicationFactory bypassFactory)
    {
        _realFactory   = realFactory;
        _bypassFactory = bypassFactory;
    }

    [Fact]
    public async Task GetWorkouts_WithoutToken_ReturnsUnauthorized()
    {
        var client   = _realFactory.CreateClient();
        var response = await client.GetAsync("/api/workouts?pageNumber=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWorkouts_WithValidToken_ReturnsOk()
    {
        var client   = _bypassFactory.CreateClient();
        var response = await client.GetAsync("/api/workouts?pageNumber=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateWorkout_WithValidToken_Returns201()
    {
        var client = _bypassFactory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/workouts", new
        {
            name  = "Morning Run",
            date  = DateTime.UtcNow,
            notes = "Integration test workout"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateWorkout_ThenGetWorkouts_ReturnsThatWorkout()
    {
        var client = _bypassFactory.CreateClient();

        await client.PostAsJsonAsync("/api/workouts", new
        {
            name  = "Evening Gym",
            date  = DateTime.UtcNow,
            notes = "Pull day"
        });

        var listResp = await client.GetAsync("/api/workouts?pageNumber=1&pageSize=20");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await listResp.Content.ReadAsStringAsync();
        body.Should().Contain("Evening Gym");
    }
}

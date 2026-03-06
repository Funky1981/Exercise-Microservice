using System.Net;
using System.Net.Http.Json;
using Exercise.API.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Exercise.API.IntegrationTests.Tests.WorkoutPlans;

/// <summary>
/// Integration tests for the /api/workout-plans endpoints.
/// Unauthorized tests use the real JWT factory; authorized tests bypass JWT via TestAuthHandler.
/// </summary>
public class WorkoutPlanEndpointTests : IClassFixture<ExerciseWebApplicationFactory>,
                                        IClassFixture<AuthBypassWebApplicationFactory>
{
    private readonly ExerciseWebApplicationFactory   _realFactory;
    private readonly AuthBypassWebApplicationFactory _bypassFactory;

    public WorkoutPlanEndpointTests(
        ExerciseWebApplicationFactory   realFactory,
        AuthBypassWebApplicationFactory bypassFactory)
    {
        _realFactory   = realFactory;
        _bypassFactory = bypassFactory;
    }

    [Fact]
    public async Task GetWorkoutPlans_WithoutToken_ReturnsUnauthorized()
    {
        var client   = _realFactory.CreateClient();
        var response = await client.GetAsync("/api/workout-plans?pageNumber=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWorkoutPlans_WithValidToken_ReturnsOk()
    {
        var client   = _bypassFactory.CreateClient();
        var response = await client.GetAsync("/api/workout-plans?pageNumber=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateWorkoutPlan_WithValidToken_Returns201()
    {
        var client = _bypassFactory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/workout-plans", new
        {
            name      = "Sprint Training",
            startDate = DateTime.UtcNow,
            notes     = "Integration test plan"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateWorkoutPlan_ThenGetWorkoutPlans_ReturnsThatPlan()
    {
        var client = _bypassFactory.CreateClient();

        await client.PostAsJsonAsync("/api/workout-plans", new
        {
            name      = "Endurance Block",
            startDate = DateTime.UtcNow,
            notes     = "12-week base build"
        });

        var listResp = await client.GetAsync("/api/workout-plans?pageNumber=1&pageSize=20");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await listResp.Content.ReadAsStringAsync();
        body.Should().Contain("Endurance Block");
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Exercise.API.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Exercise.API.IntegrationTests.Tests.WorkoutPlans;

/// <summary>
/// Integration tests for the /api/workout-plans endpoints.
/// Unauthorized tests use the real JWT factory; authorized tests bypass JWT via TestAuthHandler.
/// </summary>
[Collection("Integration")]
public class WorkoutPlanEndpointTests : DualFactoryIntegrationTestBase
{
    public WorkoutPlanEndpointTests(
        ExerciseWebApplicationFactory   realFactory,
        AuthBypassWebApplicationFactory bypassFactory)
        : base(realFactory, bypassFactory) { }

    [Fact]
    public async Task GetWorkoutPlans_WithoutToken_ReturnsUnauthorized()
    {
        var client   = RealFactory.CreateClient();
        var response = await client.GetAsync("/api/workout-plans?pageNumber=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWorkoutPlans_WithValidToken_ReturnsOk()
    {
        var client   = BypassFactory.CreateClient();
        var response = await client.GetAsync("/api/workout-plans?pageNumber=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateWorkoutPlan_WithValidToken_Returns201()
    {
        var client = BypassFactory.CreateClient();

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
        var client = BypassFactory.CreateClient();

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

    [Fact]
    public async Task GetWorkoutPlanById_ReturnsAttachedWorkouts()
    {
        var client = BypassFactory.CreateClient();
        var workoutId = await CreateWorkoutAsync(client, "Plan linked workout");
        var planId = await CreateWorkoutPlanAsCurrentUserAsync(client);

        var addResponse = await client.PostAsJsonAsync($"/api/workout-plans/{planId}/workouts", new
        {
            workoutId
        });
        addResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await client.GetAsync($"/api/workout-plans/{planId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("workouts").EnumerateArray()
            .Select(item => item.GetProperty("id").GetGuid())
            .Should()
            .Contain(workoutId);
    }

    // ── Ownership guard (403) ────────────────────────────────────────────────

    private async Task<Guid> CreateWorkoutPlanAsCurrentUserAsync(HttpClient client)
    {
        var resp = await client.PostAsJsonAsync("/api/workout-plans", new
        {
            name      = "Ownership Test Plan",
            startDate = DateTime.UtcNow,
            notes     = "ownership guard"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateWorkoutAsync(HttpClient client, string name)
    {
        var exerciseResponse = await client.PostAsJsonAsync("/api/exercises", new
        {
            name = $"{name} Exercise",
            bodyPart = "chest",
            targetMuscle = "pectorals",
            equipment = "machine",
            description = "Integration test exercise for workout plans"
        });

        exerciseResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var exerciseBody = await exerciseResponse.Content.ReadFromJsonAsync<JsonElement>();
        var exerciseId = exerciseBody.GetProperty("id").GetGuid();

        var response = await client.PostAsJsonAsync("/api/workouts", new
        {
            name,
            date = DateTime.UtcNow,
            hasExplicitTime = false,
            notes = "Created for workout plan integration test",
            exerciseIds = new[] { exerciseId }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task GetWorkoutPlanById_ByDifferentUser_ReturnsForbidden()
    {
        var original = BypassFactory.AuthenticatedUserId;
        try
        {
            BypassFactory.AuthenticatedUserId = Guid.NewGuid();
            var client = BypassFactory.CreateClient();
            var planId = await CreateWorkoutPlanAsCurrentUserAsync(client);

            BypassFactory.AuthenticatedUserId = Guid.NewGuid();
            var response = await client.GetAsync($"/api/workout-plans/{planId}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        finally { BypassFactory.AuthenticatedUserId = original; }
    }

    [Fact]
    public async Task UpdateWorkoutPlan_ByDifferentUser_ReturnsForbidden()
    {
        var original = BypassFactory.AuthenticatedUserId;
        try
        {
            BypassFactory.AuthenticatedUserId = Guid.NewGuid();
            var client = BypassFactory.CreateClient();
            var planId = await CreateWorkoutPlanAsCurrentUserAsync(client);

            BypassFactory.AuthenticatedUserId = Guid.NewGuid();
            var response = await client.PutAsJsonAsync($"/api/workout-plans/{planId}", new
            {
                name      = "Hacked Plan",
                startDate = DateTime.UtcNow,
                notes     = "should be blocked"
            });

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        finally { BypassFactory.AuthenticatedUserId = original; }
    }

    [Fact]
    public async Task DeleteWorkoutPlan_ByDifferentUser_ReturnsForbidden()
    {
        var original = BypassFactory.AuthenticatedUserId;
        try
        {
            BypassFactory.AuthenticatedUserId = Guid.NewGuid();
            var client = BypassFactory.CreateClient();
            var planId = await CreateWorkoutPlanAsCurrentUserAsync(client);

            BypassFactory.AuthenticatedUserId = Guid.NewGuid();
            var response = await client.DeleteAsync($"/api/workout-plans/{planId}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        finally { BypassFactory.AuthenticatedUserId = original; }
    }

    [Fact]
    public async Task DeleteWorkoutPlan_WithMissingId_ReturnsNotFound()
    {
        var client = BypassFactory.CreateClient();

        var response = await client.DeleteAsync($"/api/workout-plans/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Exercise.API.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Exercise.API.IntegrationTests.Tests.Workouts;

/// <summary>
/// Integration tests for the /api/workouts endpoints.
/// Unauthorized tests use the real JWT factory; authorized tests bypass JWT via TestAuthHandler.
/// </summary>
[Collection("Integration")]
public class WorkoutsEndpointTests : DualFactoryIntegrationTestBase
{
    public WorkoutsEndpointTests(
        ExerciseWebApplicationFactory   realFactory,
        AuthBypassWebApplicationFactory bypassFactory)
        : base(realFactory, bypassFactory) { }

    [Fact]
    public async Task GetWorkouts_WithoutToken_ReturnsUnauthorized()
    {
        var client   = RealFactory.CreateClient();
        var response = await client.GetAsync("/api/workouts?pageNumber=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWorkouts_WithValidToken_ReturnsOk()
    {
        var client   = BypassFactory.CreateClient();
        var response = await client.GetAsync("/api/workouts?pageNumber=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateWorkout_WithValidToken_Returns201()
    {
        var client = BypassFactory.CreateClient();

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
        var client = BypassFactory.CreateClient();

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

    [Fact]
    public async Task GetWorkoutById_ReturnsExercisesWhenWorkoutHasAssociations()
    {
        var client = BypassFactory.CreateClient();
        var exerciseId = await CreateExerciseAsync(client, "Front Squat");
        var workoutId = await CreateWorkoutAsCurrentUserAsync(client);

        var addResponse = await client.PostAsJsonAsync($"/api/workouts/{workoutId}/exercises", new
        {
            exerciseId
        });
        addResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await client.GetAsync($"/api/workouts/{workoutId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("exercises").EnumerateArray()
            .Select(item => item.GetProperty("id").GetGuid())
            .Should()
            .Contain(exerciseId);
    }

    // ── Ownership guard (403) ────────────────────────────────────────────────

    private async Task<Guid> CreateWorkoutAsCurrentUserAsync(HttpClient client)
    {
        var resp = await client.PostAsJsonAsync("/api/workouts", new
        {
            name  = "Ownership Test Workout",
            date  = DateTime.UtcNow,
            notes = "ownership guard"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private static async Task<Guid> CreateExerciseAsync(HttpClient client, string name)
    {
        var response = await client.PostAsJsonAsync("/api/exercises", new
        {
            name,
            bodyPart = "legs",
            targetMuscle = "quadriceps",
            equipment = "barbell",
            description = "Integration test exercise"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task GetWorkoutById_ByDifferentUser_ReturnsForbidden()
    {
        var original = BypassFactory.AuthenticatedUserId;
        try
        {
            BypassFactory.AuthenticatedUserId = Guid.NewGuid();
            var client = BypassFactory.CreateClient();
            var workoutId = await CreateWorkoutAsCurrentUserAsync(client);

            BypassFactory.AuthenticatedUserId = Guid.NewGuid(); // different user
            var response = await client.GetAsync($"/api/workouts/{workoutId}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        finally { BypassFactory.AuthenticatedUserId = original; }
    }

    [Fact]
    public async Task UpdateWorkout_ByDifferentUser_ReturnsForbidden()
    {
        var original = BypassFactory.AuthenticatedUserId;
        try
        {
            BypassFactory.AuthenticatedUserId = Guid.NewGuid();
            var client = BypassFactory.CreateClient();
            var workoutId = await CreateWorkoutAsCurrentUserAsync(client);

            BypassFactory.AuthenticatedUserId = Guid.NewGuid();
            var response = await client.PutAsJsonAsync($"/api/workouts/{workoutId}", new
            {
                name  = "Hacked Workout",
                date  = DateTime.UtcNow,
                notes = "should be blocked"
            });

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        finally { BypassFactory.AuthenticatedUserId = original; }
    }

    [Fact]
    public async Task DeleteWorkout_ByDifferentUser_ReturnsForbidden()
    {
        var original = BypassFactory.AuthenticatedUserId;
        try
        {
            BypassFactory.AuthenticatedUserId = Guid.NewGuid();
            var client = BypassFactory.CreateClient();
            var workoutId = await CreateWorkoutAsCurrentUserAsync(client);

            BypassFactory.AuthenticatedUserId = Guid.NewGuid();
            var response = await client.DeleteAsync($"/api/workouts/{workoutId}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
        finally { BypassFactory.AuthenticatedUserId = original; }
    }

    [Fact]
    public async Task DeleteWorkout_WithMissingId_ReturnsNotFound()
    {
        var client = BypassFactory.CreateClient();

        var response = await client.DeleteAsync($"/api/workouts/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

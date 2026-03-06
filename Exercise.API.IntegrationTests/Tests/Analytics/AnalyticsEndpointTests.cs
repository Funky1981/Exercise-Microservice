using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Exercise.API.IntegrationTests.Infrastructure;
using Exercise.Application.Common.Models;
using Exercise.Application.Features.Analytics.Dtos;
using FluentAssertions;

namespace Exercise.API.IntegrationTests.Tests.Analytics;

/// <summary>
/// Integration tests for the /api/analytics endpoints.
/// The endpoint derives UserId from the JWT sub claim, so every call is self-scoped.
/// </summary>
[Collection("Integration")]
public class AnalyticsEndpointTests : IClassFixture<ExerciseWebApplicationFactory>,
                                      IClassFixture<AuthBypassWebApplicationFactory>
{
    private readonly ExerciseWebApplicationFactory   _realFactory;
    private readonly AuthBypassWebApplicationFactory _bypassFactory;

    public AnalyticsEndpointTests(
        ExerciseWebApplicationFactory   realFactory,
        AuthBypassWebApplicationFactory bypassFactory)
    {
        _realFactory   = realFactory;
        _bypassFactory = bypassFactory;
    }

    [Fact]
    public async Task GetWorkoutSummary_WithoutToken_ReturnsUnauthorized()
    {
        var client   = _realFactory.CreateClient();
        var response = await client.GetAsync("/api/analytics/workout-summary");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWorkoutSummary_WithValidToken_ReturnsOk()
    {
        var client   = _bypassFactory.CreateClient();
        var response = await client.GetAsync("/api/analytics/workout-summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetWorkoutSummary_WithValidToken_ReturnsValidSummaryShape()
    {
        var client   = _bypassFactory.CreateClient();
        var response = await client.GetAsync("/api/analytics/workout-summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("totalWorkouts");
        body.Should().Contain("completedWorkouts");
        body.Should().Contain("totalExerciseLogs");
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Exercise.API.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace Exercise.API.IntegrationTests.Tests.Exercises;

/// <summary>
/// Integration tests for the /api/exercises endpoints.
/// Unauthorized tests use the real JWT factory; authorized tests bypass JWT via TestAuthHandler.
/// </summary>
[Collection("Integration")]
public class ExercisesEndpointTests : IClassFixture<ExerciseWebApplicationFactory>,
                                      IClassFixture<AuthBypassWebApplicationFactory>
{
    private readonly ExerciseWebApplicationFactory    _realFactory;
    private readonly AuthBypassWebApplicationFactory  _bypassFactory;

    public ExercisesEndpointTests(
        ExerciseWebApplicationFactory   realFactory,
        AuthBypassWebApplicationFactory bypassFactory)
    {
        _realFactory   = realFactory;
        _bypassFactory = bypassFactory;
    }

    [Fact]
    public async Task GetExercises_WithoutToken_ReturnsUnauthorized()
    {
        var client   = _realFactory.CreateClient();
        var response = await client.GetAsync("/api/exercises");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetExercises_WithValidToken_ReturnsOk()
    {
        var client   = _bypassFactory.CreateClient();
        var response = await client.GetAsync("/api/exercises");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetExercisesByBodyPart_WithValidToken_ReturnsOk()
    {
        var client   = _bypassFactory.CreateClient();
        var response = await client.GetAsync("/api/exercises/bodypart/chest");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetExercises_ResponseHasPagedEnvelopeShape()
    {
        var client   = _bypassFactory.CreateClient();
        var response = await client.GetAsync("/api/exercises?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.TryGetProperty("items",       out _).Should().BeTrue("response must contain 'items'");
        root.TryGetProperty("totalCount",  out _).Should().BeTrue("response must contain 'totalCount'");
        root.TryGetProperty("pageNumber",  out _).Should().BeTrue("response must contain 'pageNumber'");
        root.TryGetProperty("pageSize",    out _).Should().BeTrue("response must contain 'pageSize'");
        root.TryGetProperty("totalPages",  out _).Should().BeTrue("response must contain 'totalPages'");
        root.TryGetProperty("hasNextPage", out _).Should().BeTrue("response must contain 'hasNextPage'");
    }
}

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
public class ExercisesEndpointTests : DualFactoryIntegrationTestBase
{
    public ExercisesEndpointTests(
        ExerciseWebApplicationFactory   realFactory,
        AuthBypassWebApplicationFactory bypassFactory)
        : base(realFactory, bypassFactory) { }

    [Fact]
    public async Task GetExercises_WithoutToken_ReturnsUnauthorized()
    {
        var client   = RealFactory.CreateClient();
        var response = await client.GetAsync("/api/exercises");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetExercises_WithValidToken_ReturnsOk()
    {
        var client   = BypassFactory.CreateClient();
        var response = await client.GetAsync("/api/exercises");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetExercisesByBodyPart_WithValidToken_ReturnsOk()
    {
        var client   = BypassFactory.CreateClient();
        var response = await client.GetAsync("/api/exercises/bodypart/chest");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetExercises_ResponseHasPagedEnvelopeShape()
    {
        var client   = BypassFactory.CreateClient();
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

    [Fact]
    public async Task GetExercises_WithRegionFilter_ReturnsExercisesFromThatRegion()
    {
        var client = BypassFactory.CreateClient();
        await CreateExerciseAsync(client, "Bench Press", "chest", "pectorals");
        await CreateExerciseAsync(client, "Lat Pulldown", "back", "lats");
        await CreateExerciseAsync(client, "Back Squat", "upper legs", "quadriceps");

        var response = await client.GetAsync("/api/exercises?region=upper-body&pageNumber=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var bodyParts = body.GetProperty("items").EnumerateArray()
            .Select(item => item.GetProperty("bodyPart").GetString())
            .Where(value => value is not null)
            .Cast<string>()
            .ToList();

        bodyParts.Should().Contain(["chest", "back"]);
        bodyParts.Should().NotContain("upper legs");
    }

    [Fact]
    public async Task GetExerciseFilters_ReturnsRegionsBodyPartsAndEquipment()
    {
        var client = BypassFactory.CreateClient();
        var response = await client.GetAsync("/api/exercises/filters");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.TryGetProperty("regions", out _).Should().BeTrue();
        body.TryGetProperty("bodyPartsByRegion", out _).Should().BeTrue();
        body.TryGetProperty("equipment", out _).Should().BeTrue();
    }

    private static async Task<Guid> CreateExerciseAsync(
        HttpClient client,
        string name,
        string bodyPart,
        string targetMuscle)
    {
        var response = await client.PostAsJsonAsync("/api/exercises", new
        {
            name,
            bodyPart,
            targetMuscle,
            equipment = "bodyweight",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }
}

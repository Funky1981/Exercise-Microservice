using Exercise.Application.Abstractions.Repositories;
using System.Text.Json;
using System.Text.Json.Serialization;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;

namespace Exercise.API
{
    public static class RapidApiEndpointMapper
    {
        public static void MapRapidApiEndpoints(this WebApplication app)
        {
            app.MapGet("/api/exercises/sync",
                async (IHttpClientFactory httpClientFactory,
                       IExerciseRepository exerciseRepository,
                       IUnitOfWork unitOfWork,
                       ILogger<Program> logger,
                       CancellationToken ct) =>
                {
                    var client = httpClientFactory.CreateClient("ExerciseApi");

                    List<RapidApiExercise> remoteExercises;
                    try
                    {
                        var response = await client.GetAsync("exercises?limit=100&offset=0", ct);
                        response.EnsureSuccessStatusCode();

                        var json = await response.Content.ReadAsStringAsync(ct);
                        remoteExercises = JsonSerializer.Deserialize<List<RapidApiExercise>>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                            ?? [];
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to fetch exercises from RapidAPI.");
                        return Results.Problem(
                            detail: "Could not reach the external exercise database. Please try again later.",
                            statusCode: StatusCodes.Status502BadGateway);
                    }

                    var existing = await exerciseRepository.GetAllAsync(ct);
                    var existingNames = existing
                        .Select(e => e.Name.ToLowerInvariant())
                        .ToHashSet();

                    int added = 0;
                    foreach (var remote in remoteExercises)
                    {
                        if (string.IsNullOrWhiteSpace(remote.Name)
                            || string.IsNullOrWhiteSpace(remote.BodyPart)
                            || string.IsNullOrWhiteSpace(remote.Target))
                            continue;

                        if (existingNames.Contains(remote.Name.ToLowerInvariant()))
                            continue;

                        var exercise = new ExerciseEntity(
                            Guid.NewGuid(),
                            remote.Name,
                            remote.BodyPart,
                            remote.Target,
                            equipment: remote.Equipment,
                            gifUrl: remote.GifUrl);

                        await exerciseRepository.AddAsync(exercise, ct);
                        added++;
                    }

                    if (added > 0)
                        await unitOfWork.SaveChangesAsync(ct);

                    logger.LogInformation("RapidAPI sync complete. {Added} new exercises added.", added);

                    return Results.Ok(new { synced = added, total = remoteExercises.Count });
                })
            .WithTags("Exercises")
            .WithOpenApi()
            .WithName("SyncExercisesFromRapidApi")
            .WithSummary("Sync exercises from the RapidAPI ExerciseDB (admin use)")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status502BadGateway);
        }

        private sealed record RapidApiExercise(
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("bodyPart")] string BodyPart,
            [property: JsonPropertyName("target")] string Target,
            [property: JsonPropertyName("equipment")] string? Equipment,
            [property: JsonPropertyName("gifUrl")] string? GifUrl);
    }
}

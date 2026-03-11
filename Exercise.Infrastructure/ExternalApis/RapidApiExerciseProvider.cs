using Exercise.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Exercise.Infrastructure.ExternalApis
{
    /// <summary>
    /// Fetches exercise data from the RapidAPI ExerciseDB.
    /// To swap providers, implement <see cref="IExerciseDataProvider"/> and register
    /// the new implementation in DI — no other code changes required.
    /// </summary>
    public class RapidApiExerciseProvider : IExerciseDataProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RapidApiExerciseProvider> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public RapidApiExerciseProvider(
            IHttpClientFactory httpClientFactory,
            ILogger<RapidApiExerciseProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ExternalExerciseDto>> FetchExercisesAsync(
            int limit, int offset, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("ExerciseApi");

            var response = await client.GetAsync(
                $"exercises?limit={limit}&offset={offset}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var exercises = JsonSerializer.Deserialize<List<RapidApiExercise>>(json, JsonOptions) ?? [];

            _logger.LogInformation("Fetched {Count} exercises from RapidAPI (limit={Limit}, offset={Offset}).",
                exercises.Count, limit, offset);

            return exercises
                .Select(e => new ExternalExerciseDto(
                    e.Name,
                    e.BodyPart,
                    e.Target,
                    e.Equipment,
                    e.GifUrl))
                .ToList()
                .AsReadOnly();
        }

        private sealed record RapidApiExercise(
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("bodyPart")] string BodyPart,
            [property: JsonPropertyName("target")] string Target,
            [property: JsonPropertyName("equipment")] string? Equipment,
            [property: JsonPropertyName("gifUrl")] string? GifUrl);
    }
}

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
            using var document = JsonDocument.Parse(json);
            var exercises = ParseExercises(document.RootElement);

            _logger.LogInformation("Fetched {Count} exercises from RapidAPI (limit={Limit}, offset={Offset}).",
                exercises.Count, limit, offset);

            return exercises;
        }

        private static IReadOnlyList<ExternalExerciseDto> ParseExercises(JsonElement root)
        {
            if (root.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var exercises = new List<ExternalExerciseDto>();
            foreach (var item in root.EnumerateArray())
            {
                exercises.Add(new ExternalExerciseDto(
                    GetString(item, "id"),
                    GetString(item, "name") ?? string.Empty,
                    GetString(item, "bodyPart") ?? string.Empty,
                    GetString(item, "target") ?? string.Empty,
                    GetString(item, "equipment"),
                    GetString(item, "gifUrl"),
                    GetString(item, "gifUrl"),
                    string.IsNullOrWhiteSpace(GetString(item, "gifUrl")) ? null : "image/gif",
                    GetStringArray(item, "secondaryMuscles"),
                    GetStringArray(item, "instructions"),
                    item.GetRawText(),
                    "RapidApiExerciseDb",
                    GetString(item, "description"),
                    GetString(item, "difficulty"),
                    GetString(item, "category")));
            }

            return exercises.AsReadOnly();
        }

        private static string? GetString(JsonElement item, string propertyName)
        {
            if (!item.TryGetProperty(propertyName, out var value))
            {
                return null;
            }

            return value.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.String => value.GetString(),
                _ => value.ToString()
            };
        }

        private static IReadOnlyList<string> GetStringArray(JsonElement item, string propertyName)
        {
            if (!item.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            return value
                .EnumerateArray()
                .Select(entry => entry.GetString())
                .Where(entry => !string.IsNullOrWhiteSpace(entry))
                .Cast<string>()
                .ToList()
                .AsReadOnly();
        }
    }
}

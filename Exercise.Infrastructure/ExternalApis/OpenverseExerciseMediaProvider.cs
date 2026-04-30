using Exercise.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Exercise.Infrastructure.ExternalApis
{
    public sealed class OpenverseExerciseMediaProvider : IExerciseMediaProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OpenverseExerciseMediaProvider> _logger;

        public OpenverseExerciseMediaProvider(
            IHttpClientFactory httpClientFactory,
            ILogger<OpenverseExerciseMediaProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public string ProviderName => "Openverse";

        public bool IsConfigured => true;

        public async Task<ExternalExerciseMediaDto?> FindMediaAsync(ExerciseMediaSearchQuery query, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("OpenverseExerciseApi");
            var response = await client.GetAsync($"images/?q={Uri.EscapeDataString(query.Name)}&page_size=5", cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
            {
                return null;
            }

            ExternalExerciseMediaDto? bestMatch = null;
            foreach (var item in results.EnumerateArray())
            {
                var mediaUrl = GetString(item, "url");
                if (string.IsNullOrWhiteSpace(mediaUrl))
                {
                    continue;
                }

                var sourceTitle = GetString(item, "title");
                var sourcePageUrl = GetString(item, "foreign_landing_url") ?? GetString(item, "detail_url");
                var score = ExerciseMediaMatchScorer.Score(query, sourceTitle, sourcePageUrl, ProviderName);

                var candidate = new ExternalExerciseMediaDto(
                    mediaUrl,
                    "image/jpeg",
                    GetString(item, "thumbnail"),
                    sourcePageUrl,
                    ProviderName,
                    item.GetRawText(),
                    sourceTitle,
                    score);

                if (bestMatch is null || candidate.MatchScore > bestMatch.MatchScore)
                {
                    bestMatch = candidate;
                }
            }

            if (bestMatch is not null)
            {
                _logger.LogDebug("Found Openverse media for {ExerciseName} with score {Score}.", query.Name, bestMatch.MatchScore);
            }

            return bestMatch;
        }

        private static string? GetString(JsonElement item, string propertyName)
        {
            if (!item.TryGetProperty(propertyName, out var value))
            {
                return null;
            }

            return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
        }
    }
}

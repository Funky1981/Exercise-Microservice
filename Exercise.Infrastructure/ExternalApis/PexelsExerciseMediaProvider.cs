using Exercise.Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Exercise.Infrastructure.ExternalApis
{
    public sealed class PexelsExerciseMediaProvider : IExerciseMediaProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PexelsExerciseMediaProvider> _logger;
        private readonly string? _apiKey;

        public PexelsExerciseMediaProvider(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<PexelsExerciseMediaProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _apiKey = configuration["Pexels:ApiKey"];
        }

        public string ProviderName => "Pexels";

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

        public async Task<ExternalExerciseMediaDto?> FindMediaAsync(ExerciseMediaSearchQuery query, CancellationToken cancellationToken = default)
        {
            if (!IsConfigured)
            {
                return null;
            }

            var client = _httpClientFactory.CreateClient("PexelsExerciseApi");
            ExternalExerciseMediaDto? bestMatch = null;

            var videoResponse = await client.GetAsync($"videos/search?query={Uri.EscapeDataString(query.Name)}&per_page=5", cancellationToken);
            videoResponse.EnsureSuccessStatusCode();
            var videoJson = await videoResponse.Content.ReadAsStringAsync(cancellationToken);
            using (var videoDocument = JsonDocument.Parse(videoJson))
            {
                if (videoDocument.RootElement.TryGetProperty("videos", out var videos)
                    && videos.ValueKind == JsonValueKind.Array
                    && videos.GetArrayLength() > 0)
                {
                    foreach (var item in videos.EnumerateArray())
                    {
                        var mediaUrl = GetPreferredVideoUrl(item);
                        if (string.IsNullOrWhiteSpace(mediaUrl))
                        {
                            continue;
                        }

                        var sourcePageUrl = GetString(item, "url");
                        var sourceTitle = ExerciseMediaMatchScorer.Slugify(sourcePageUrl);
                        var candidate = new ExternalExerciseMediaDto(
                            mediaUrl,
                            "video/mp4",
                            GetString(item, "image"),
                            sourcePageUrl,
                            ProviderName,
                            item.GetRawText(),
                            sourceTitle,
                            ExerciseMediaMatchScorer.Score(query, sourceTitle, sourcePageUrl, ProviderName));

                        if (bestMatch is null || candidate.MatchScore > bestMatch.MatchScore)
                        {
                            bestMatch = candidate;
                        }
                    }
                }
            }

            var photoResponse = await client.GetAsync($"v1/search?query={Uri.EscapeDataString(query.Name)}&per_page=5", cancellationToken);
            photoResponse.EnsureSuccessStatusCode();
            var photoJson = await photoResponse.Content.ReadAsStringAsync(cancellationToken);
            using var photoDocument = JsonDocument.Parse(photoJson);
            if (!photoDocument.RootElement.TryGetProperty("photos", out var photos)
                || photos.ValueKind != JsonValueKind.Array
                || photos.GetArrayLength() == 0)
            {
                return bestMatch;
            }

            foreach (var photo in photos.EnumerateArray())
            {
                if (!photo.TryGetProperty("src", out var src))
                {
                    continue;
                }

                var imageUrl = GetString(src, "large") ?? GetString(src, "medium") ?? GetString(src, "original");
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    continue;
                }

                var sourcePageUrl = GetString(photo, "url");
                var sourceTitle = GetString(photo, "alt") ?? ExerciseMediaMatchScorer.Slugify(sourcePageUrl);
                var candidate = new ExternalExerciseMediaDto(
                    imageUrl,
                    "image/jpeg",
                    GetString(src, "small") ?? GetString(src, "tiny"),
                    sourcePageUrl,
                    ProviderName,
                    photo.GetRawText(),
                    sourceTitle,
                    ExerciseMediaMatchScorer.Score(query, sourceTitle, sourcePageUrl, ProviderName));

                if (bestMatch is null || candidate.MatchScore > bestMatch.MatchScore)
                {
                    bestMatch = candidate;
                }
            }

            if (bestMatch is not null)
            {
                _logger.LogDebug("Found Pexels media for {ExerciseName} with score {Score}.", query.Name, bestMatch.MatchScore);
            }

            return bestMatch;
        }

        private static string? GetPreferredVideoUrl(JsonElement item)
        {
            if (!item.TryGetProperty("video_files", out var videoFiles) || videoFiles.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (var videoFile in videoFiles.EnumerateArray())
            {
                var link = GetString(videoFile, "link");
                var quality = GetString(videoFile, "quality");
                if (!string.IsNullOrWhiteSpace(link) && string.Equals(quality, "sd", StringComparison.OrdinalIgnoreCase))
                {
                    return link;
                }
            }

            foreach (var videoFile in videoFiles.EnumerateArray())
            {
                var link = GetString(videoFile, "link");
                if (!string.IsNullOrWhiteSpace(link))
                {
                    return link;
                }
            }

            return null;
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

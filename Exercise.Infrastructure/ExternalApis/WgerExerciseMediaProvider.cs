using Exercise.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Exercise.Infrastructure.ExternalApis
{
    public sealed class WgerExerciseMediaProvider : IExerciseMediaProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WgerExerciseMediaProvider> _logger;

        public WgerExerciseMediaProvider(
            IHttpClientFactory httpClientFactory,
            ILogger<WgerExerciseMediaProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public string ProviderName => "Wger";

        public bool IsConfigured => true;

        public async Task<ExternalExerciseMediaDto?> FindMediaAsync(ExerciseMediaSearchQuery query, CancellationToken cancellationToken = default)
        {
            if (!string.Equals(query.SourceProvider, ProviderName, StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(query.SourcePayloadJson))
            {
                return null;
            }

            var exerciseId = TryGetExerciseId(query.SourcePayloadJson);
            if (exerciseId is null)
            {
                return null;
            }

            var client = _httpClientFactory.CreateClient("WgerExerciseApi");

            var videoResponse = await client.GetAsync($"video/?limit=3&exercise={exerciseId}", cancellationToken);
            videoResponse.EnsureSuccessStatusCode();
            var videoJson = await videoResponse.Content.ReadAsStringAsync(cancellationToken);
            using (var videoDocument = JsonDocument.Parse(videoJson))
            {
                if (videoDocument.RootElement.TryGetProperty("results", out var videos)
                    && videos.ValueKind == JsonValueKind.Array
                    && videos.GetArrayLength() > 0)
                {
                    var video = videos[0];
                    var mediaUrl = GetString(video, "video");
                    if (!string.IsNullOrWhiteSpace(mediaUrl))
                    {
                        _logger.LogDebug("Found wger video for {ExerciseName}.", query.Name);
                        return new ExternalExerciseMediaDto(
                            mediaUrl,
                            InferMediaKind(mediaUrl),
                            null,
                            mediaUrl,
                            ProviderName,
                            video.GetRawText(),
                            query.Name,
                            0.99d);
                    }
                }
            }

            return null;
        }

        private static int? TryGetExerciseId(string sourcePayloadJson)
        {
            try
            {
                using var document = JsonDocument.Parse(sourcePayloadJson);
                if (document.RootElement.TryGetProperty("id", out var idElement)
                    && idElement.ValueKind == JsonValueKind.Number
                    && idElement.TryGetInt32(out var exerciseId))
                {
                    return exerciseId;
                }
            }
            catch
            {
                return null;
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

        private static string InferMediaKind(string mediaUrl)
        {
            var lower = mediaUrl.ToLowerInvariant();
            if (lower.EndsWith(".mov")) return "video/quicktime";
            if (lower.EndsWith(".mp4")) return "video/mp4";
            if (lower.EndsWith(".webm")) return "video/webm";
            return "video/mp4";
        }
    }
}
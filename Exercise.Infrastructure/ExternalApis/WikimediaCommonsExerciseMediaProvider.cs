using Exercise.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Exercise.Infrastructure.ExternalApis
{
    public sealed class WikimediaCommonsExerciseMediaProvider : IExerciseMediaProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WikimediaCommonsExerciseMediaProvider> _logger;

        public WikimediaCommonsExerciseMediaProvider(
            IHttpClientFactory httpClientFactory,
            ILogger<WikimediaCommonsExerciseMediaProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public string ProviderName => "WikimediaCommons";

        public bool IsConfigured => true;

        public async Task<ExternalExerciseMediaDto?> FindMediaAsync(ExerciseMediaSearchQuery query, CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("WikimediaCommonsApi");
            var requestUri = "api.php?action=query&generator=search&gsrnamespace=6&gsrlimit=5&prop=imageinfo&iiprop=url&iiurlwidth=400&format=json&formatversion=2&origin=*&gsrsearch="
                + Uri.EscapeDataString(query.Name + " exercise");
            var response = await client.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("query", out var queryNode)
                || !queryNode.TryGetProperty("pages", out var pages)
                || pages.ValueKind != JsonValueKind.Array
                || pages.GetArrayLength() == 0)
            {
                return null;
            }

            ExternalExerciseMediaDto? bestMatch = null;
            foreach (var page in pages.EnumerateArray())
            {
                if (!page.TryGetProperty("imageinfo", out var imageInfo)
                    || imageInfo.ValueKind != JsonValueKind.Array
                    || imageInfo.GetArrayLength() == 0)
                {
                    continue;
                }

                var info = imageInfo[0];
                var mediaUrl = GetString(info, "url");
                if (string.IsNullOrWhiteSpace(mediaUrl))
                {
                    continue;
                }

                var sourceTitle = GetString(page, "title");
                var sourcePageUrl = GetString(page, "canonicalurl") ?? GetString(info, "descriptionurl");
                var score = ExerciseMediaMatchScorer.Score(query, sourceTitle, sourcePageUrl, ProviderName);

                var candidate = new ExternalExerciseMediaDto(
                    mediaUrl,
                    InferMediaKind(mediaUrl),
                    GetString(info, "thumburl"),
                    sourcePageUrl,
                    ProviderName,
                    page.GetRawText(),
                    sourceTitle,
                    score);

                if (bestMatch is null || candidate.MatchScore > bestMatch.MatchScore)
                {
                    bestMatch = candidate;
                }
            }

            if (bestMatch is not null)
            {
                _logger.LogDebug("Found Wikimedia Commons media for {ExerciseName} with score {Score}.", query.Name, bestMatch.MatchScore);
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

        private static string InferMediaKind(string mediaUrl)
        {
            var lower = mediaUrl.ToLowerInvariant();
            if (lower.EndsWith(".gif")) return "image/gif";
            if (lower.EndsWith(".png")) return "image/png";
            if (lower.EndsWith(".webm")) return "video/webm";
            if (lower.EndsWith(".mp4")) return "video/mp4";
            return "image/jpeg";
        }
    }
}

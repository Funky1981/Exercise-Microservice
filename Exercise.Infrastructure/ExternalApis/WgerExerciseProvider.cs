using Exercise.Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Exercise.Infrastructure.ExternalApis
{
    public sealed class WgerExerciseProvider : IExerciseDataProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WgerExerciseProvider> _logger;
        private readonly int _preferredLanguage;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public WgerExerciseProvider(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<WgerExerciseProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _preferredLanguage = Math.Max(1, configuration.GetValue<int?>("Wger:PreferredLanguage") ?? 2);
        }

        public async Task<IReadOnlyList<ExternalExerciseDto>> FetchExercisesAsync(
            int limit,
            int offset,
            CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("WgerExerciseApi");
            var response = await client.GetAsync($"exerciseinfo/?limit={limit}&offset={offset}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(json);
            var exercises = ParseExercises(document.RootElement, _preferredLanguage);

            _logger.LogInformation("Fetched {Count} exercises from wger (limit={Limit}, offset={Offset}).",
                exercises.Count, limit, offset);

            return exercises;
        }

        private static IReadOnlyList<ExternalExerciseDto> ParseExercises(JsonElement root, int preferredLanguage)
        {
            if (!root.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var exercises = new List<ExternalExerciseDto>();
            foreach (var item in results.EnumerateArray())
            {
                var translation = SelectTranslation(item, preferredLanguage);
                var description = StripHtml(GetString(translation, "description"));
                var name = GetString(translation, "name") ?? GetString(item, "name") ?? string.Empty;
                var bodyPart = GetNestedString(item, "category", "name") ?? "Other";
                var targetMuscle = GetFirstArrayObjectString(item, "muscles", "name") ?? bodyPart;
                var equipment = JoinArrayObjectNames(item, "equipment");
                var videoUrl = GetFirstArrayObjectString(item, "videos", "video")
                    ?? GetFirstArrayObjectString(item, "videos", "url");
                var imageUrl = GetFirstArrayObjectString(item, "images", "image")
                    ?? GetFirstArrayObjectString(item, "images", "url");
                var mediaUrl = videoUrl ?? imageUrl;
                var mediaKind = videoUrl is not null ? "video/mp4" : imageUrl is not null ? "image/jpeg" : null;
                var instructions = string.IsNullOrWhiteSpace(description)
                    ? []
                    : SplitParagraphs(description);

                exercises.Add(new ExternalExerciseDto(
                    GetString(item, "uuid") ?? GetString(item, "id"),
                    name,
                    bodyPart,
                    targetMuscle,
                    equipment,
                    imageUrl,
                    mediaUrl,
                    mediaKind,
                    GetArrayObjectNames(item, "muscles_secondary"),
                    instructions,
                    item.GetRawText(),
                    "Wger",
                    description,
                    null,
                    bodyPart));
            }

            return exercises.AsReadOnly();
        }

        private static JsonElement SelectTranslation(JsonElement item, int preferredLanguage)
        {
            if (!item.TryGetProperty("translations", out var translations) || translations.ValueKind != JsonValueKind.Array)
            {
                return default;
            }

            JsonElement? first = null;
            foreach (var translation in translations.EnumerateArray())
            {
                first ??= translation;
                if (translation.TryGetProperty("language", out var language)
                    && language.ValueKind == JsonValueKind.Number
                    && language.GetInt32() == preferredLanguage)
                {
                    return translation;
                }
            }

            return first ?? default;
        }

        private static string? GetString(JsonElement item, string propertyName)
        {
            if (item.ValueKind == JsonValueKind.Undefined || !item.TryGetProperty(propertyName, out var value))
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

        private static string? GetNestedString(JsonElement item, string objectProperty, string nestedProperty)
        {
            if (!item.TryGetProperty(objectProperty, out var nested) || nested.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            return GetString(nested, nestedProperty);
        }

        private static string? GetFirstArrayObjectString(JsonElement item, string arrayProperty, string nestedProperty)
        {
            if (!item.TryGetProperty(arrayProperty, out var array) || array.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            foreach (var entry in array.EnumerateArray())
            {
                var value = GetString(entry, nestedProperty);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

        private static IReadOnlyList<string> GetArrayObjectNames(JsonElement item, string arrayProperty)
        {
            if (!item.TryGetProperty(arrayProperty, out var array) || array.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            return array
                .EnumerateArray()
                .Select(entry => GetString(entry, "name"))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Cast<string>()
                .ToList()
                .AsReadOnly();
        }

        private static string? JoinArrayObjectNames(JsonElement item, string arrayProperty)
        {
            var names = GetArrayObjectNames(item, arrayProperty);
            return names.Count == 0 ? null : string.Join(", ", names);
        }

        private static IReadOnlyList<string> SplitParagraphs(string description)
        {
            return description
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .ToList()
                .AsReadOnly();
        }

        private static string? StripHtml(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var decoded = WebUtility.HtmlDecode(value);
            var buffer = new System.Text.StringBuilder(decoded.Length);
            var insideTag = false;

            foreach (var character in decoded)
            {
                if (character == '<')
                {
                    insideTag = true;
                    continue;
                }

                if (character == '>')
                {
                    insideTag = false;
                    continue;
                }

                if (!insideTag)
                {
                    buffer.Append(character);
                }
            }

            return buffer.ToString().Trim();
        }
    }
}

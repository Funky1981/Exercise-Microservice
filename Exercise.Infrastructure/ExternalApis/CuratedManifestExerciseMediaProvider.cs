using Exercise.Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Exercise.Infrastructure.ExternalApis
{
    public sealed class CuratedManifestExerciseMediaProvider : IExerciseMediaProvider
    {
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _manifestPath;
        private readonly ILogger<CuratedManifestExerciseMediaProvider> _logger;
        private IReadOnlyList<CuratedExerciseMediaEntry>? _cachedEntries;
        private DateTime _cachedLastWriteUtc;

        public CuratedManifestExerciseMediaProvider(
            IConfiguration configuration,
            IHostEnvironment hostEnvironment,
            ILogger<CuratedManifestExerciseMediaProvider> logger)
        {
            var configuredPath = configuration["CuratedMedia:ManifestPath"];
            _manifestPath = Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(hostEnvironment.ContentRootPath, configuredPath ?? Path.Combine("CuratedMedia", "curated-exercise-videos.json"));
            _logger = logger;
        }

        public string ProviderName => "CuratedManifest";

        public bool IsConfigured => File.Exists(_manifestPath);

        public Task<ExternalExerciseMediaDto?> FindMediaAsync(ExerciseMediaSearchQuery query, CancellationToken cancellationToken = default)
        {
            if (!IsConfigured)
            {
                return Task.FromResult<ExternalExerciseMediaDto?>(null);
            }

            var entries = LoadEntries();
            if (entries.Count == 0)
            {
                return Task.FromResult<ExternalExerciseMediaDto?>(null);
            }

            var bestMatch = entries
                .Select(entry => new { Entry = entry, Score = CalculateScore(entry, query) })
                .Where(match => match.Score > 0d && !string.IsNullOrWhiteSpace(match.Entry.VideoUrl))
                .OrderByDescending(match => match.Score)
                .ThenBy(match => match.Entry.Name)
                .FirstOrDefault();

            if (bestMatch is null)
            {
                return Task.FromResult<ExternalExerciseMediaDto?>(null);
            }

            var mediaKind = !string.IsNullOrWhiteSpace(bestMatch.Entry.MediaKind)
                ? bestMatch.Entry.MediaKind
                : InferMediaKind(bestMatch.Entry.VideoUrl!);

            return Task.FromResult<ExternalExerciseMediaDto?>(new ExternalExerciseMediaDto(
                bestMatch.Entry.VideoUrl,
                mediaKind,
                bestMatch.Entry.ThumbnailUrl,
                bestMatch.Entry.SourcePageUrl ?? bestMatch.Entry.VideoUrl,
                ProviderName,
                JsonSerializer.Serialize(bestMatch.Entry, SerializerOptions),
                bestMatch.Entry.SourceTitle ?? bestMatch.Entry.Name,
                bestMatch.Score));
        }

        private IReadOnlyList<CuratedExerciseMediaEntry> LoadEntries()
        {
            var lastWriteUtc = File.GetLastWriteTimeUtc(_manifestPath);
            if (_cachedEntries is not null && lastWriteUtc == _cachedLastWriteUtc)
            {
                return _cachedEntries;
            }

            try
            {
                var json = File.ReadAllText(_manifestPath);
                _cachedEntries = JsonSerializer.Deserialize<List<CuratedExerciseMediaEntry>>(json, SerializerOptions) ?? [];
                _cachedLastWriteUtc = lastWriteUtc;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to load curated exercise video manifest from {ManifestPath}.", _manifestPath);
                _cachedEntries = [];
                _cachedLastWriteUtc = lastWriteUtc;
            }

            return _cachedEntries;
        }

        private static double CalculateScore(CuratedExerciseMediaEntry entry, ExerciseMediaSearchQuery query)
        {
            if (entry.SourceMatches is { Count: > 0 }
                && !string.IsNullOrWhiteSpace(query.SourceProvider)
                && !string.IsNullOrWhiteSpace(query.ExternalId))
            {
                var sourceMatch = entry.SourceMatches.Any(match =>
                    string.Equals(match.Provider, query.SourceProvider, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(match.ExternalId, query.ExternalId, StringComparison.OrdinalIgnoreCase));

                if (sourceMatch)
                {
                    return 1.0d;
                }
            }

            var queryName = Normalize(query.Name);
            var entryAliases = entry.Aliases?.Select(Normalize).Where(alias => alias.Length > 0).ToArray() ?? [];
            var nameMatched = Normalize(entry.Name) == queryName || entryAliases.Contains(queryName);
            if (!nameMatched)
            {
                return 0d;
            }

            var bodyPartMatched = string.IsNullOrWhiteSpace(entry.BodyPart)
                || Normalize(entry.BodyPart) == Normalize(query.BodyPart);
            var targetMatched = string.IsNullOrWhiteSpace(entry.TargetMuscle)
                || Normalize(entry.TargetMuscle) == Normalize(query.TargetMuscle);
            var equipmentMatched = string.IsNullOrWhiteSpace(entry.Equipment)
                || Normalize(entry.Equipment) == Normalize(query.Equipment);

            if (!bodyPartMatched || !targetMatched)
            {
                return 0d;
            }

            return equipmentMatched ? 0.97d : 0.94d;
        }

        private static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            Span<char> buffer = stackalloc char[value.Length];
            var index = 0;
            foreach (var character in value)
            {
                if (char.IsLetterOrDigit(character))
                {
                    buffer[index++] = char.ToLowerInvariant(character);
                }
            }

            return new string(buffer[..index]);
        }

        private static string InferMediaKind(string mediaUrl)
        {
            var lower = mediaUrl.ToLowerInvariant();
            if (lower.EndsWith(".mov")) return "video/quicktime";
            if (lower.EndsWith(".webm")) return "video/webm";
            if (lower.EndsWith(".ogg")) return "video/ogg";
            return "video/mp4";
        }

        private sealed class CuratedExerciseMediaEntry
        {
            public string Name { get; init; } = string.Empty;
            public string? BodyPart { get; init; }
            public string? TargetMuscle { get; init; }
            public string? Equipment { get; init; }
            public string? VideoUrl { get; init; }
            public string? ThumbnailUrl { get; init; }
            public string? SourcePageUrl { get; init; }
            public string? MediaKind { get; init; }
            public string? SourceTitle { get; init; }
            public IReadOnlyList<string>? Aliases { get; init; }
            public IReadOnlyList<CuratedExerciseMediaSourceMatch>? SourceMatches { get; init; }
        }

        private sealed class CuratedExerciseMediaSourceMatch
        {
            public string Provider { get; init; } = string.Empty;
            public string ExternalId { get; init; } = string.Empty;
        }
    }
}
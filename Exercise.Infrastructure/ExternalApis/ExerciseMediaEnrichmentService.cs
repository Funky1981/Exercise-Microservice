using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Abstractions.Services;
using Exercise.Application.Common;
using Exercise.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;

namespace Exercise.Infrastructure.ExternalApis
{
    public sealed class ExerciseMediaEnrichmentService : IExerciseMediaEnrichmentService
    {
        private static readonly HashSet<string> AutoAcceptProviders = new(StringComparer.OrdinalIgnoreCase)
        {
            "Wger",
            "CuratedManifest"
        };

        private readonly IReadOnlyList<IExerciseMediaProvider> _mediaProviders;
        private readonly IExerciseMediaCandidateRepository _candidateRepository;
        private readonly ExerciseProviderOptions _options;
        private readonly ILogger<ExerciseMediaEnrichmentService> _logger;

        public ExerciseMediaEnrichmentService(
            IEnumerable<IExerciseMediaProvider> mediaProviders,
            IExerciseMediaCandidateRepository candidateRepository,
            IOptions<ExerciseProviderOptions> options,
            ILogger<ExerciseMediaEnrichmentService> logger)
        {
            _mediaProviders = mediaProviders.ToList().AsReadOnly();
            _candidateRepository = candidateRepository;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<int> EnrichMissingMediaAsync(
            IReadOnlyCollection<ExerciseEntity> exercises,
            int? limit = null,
            CancellationToken cancellationToken = default)
        {
            var selectedProviders = ResolveSelectedProviders();
            if (selectedProviders.Count == 0)
            {
                return 0;
            }

            var effectiveLimit = limit ?? _options.MediaExerciseLimit;
            var candidates = exercises
                .Where(exercise => !string.IsNullOrWhiteSpace(exercise.Name) && string.IsNullOrWhiteSpace(exercise.MediaUrl))
                .OrderBy(exercise => exercise.Name)
                .Take(effectiveLimit)
                .ToList();

            var candidateLookup = (await _candidateRepository.GetByExerciseIdsForUpdateAsync(
                    candidates.Select(exercise => exercise.Id).ToList(),
                    cancellationToken))
                .GroupBy(candidate => candidate.ExerciseId)
                .ToDictionary(group => group.Key, group => group.ToList());

            var enriched = 0;
            foreach (var exercise in candidates)
            {
                if (!candidateLookup.TryGetValue(exercise.Id, out var exerciseCandidates))
                {
                    exerciseCandidates = [];
                    candidateLookup[exercise.Id] = exerciseCandidates;
                }

                var query = new ExerciseMediaSearchQuery(
                    exercise.Name,
                    exercise.BodyPart,
                    exercise.TargetMuscle,
                    exercise.Equipment,
                    exercise.ExternalId,
                    exercise.SourceProvider,
                    exercise.SourcePayloadJson);

                foreach (var provider in selectedProviders)
                {
                    ExternalExerciseMediaDto? media;
                    try
                    {
                        media = await provider.FindMediaAsync(query, cancellationToken);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogWarning(
                            exception,
                            "Media provider {ProviderName} failed for exercise {ExerciseName}. Continuing with the next provider.",
                            provider.ProviderName,
                            exercise.Name);
                        continue;
                    }

                    if (media?.MediaUrl is null)
                    {
                        continue;
                    }

                    if (!ExerciseMediaPolicy.IsSupportedExampleMedia(media.MediaKind, media.MediaUrl))
                    {
                        _logger.LogDebug(
                            "Rejected non-video media from provider {ProviderName} for exercise {ExerciseName}.",
                            provider.ProviderName,
                            exercise.Name);
                        continue;
                    }

                    if (media.MatchScore < _options.MediaCandidateMinimumScore)
                    {
                        _logger.LogDebug(
                            "Rejected low-confidence media from provider {ProviderName} for exercise {ExerciseName}. Score: {Score}.",
                            provider.ProviderName,
                            exercise.Name,
                            media.MatchScore);
                        continue;
                    }

                    var existingCandidate = exerciseCandidates.FirstOrDefault(candidate =>
                        string.Equals(candidate.SourceProvider, media.SourceProvider ?? provider.ProviderName, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(candidate.MediaUrl, media.MediaUrl, StringComparison.OrdinalIgnoreCase));

                    if (existingCandidate is null)
                    {
                        existingCandidate = new ExerciseMediaCandidate(
                            Guid.NewGuid(),
                            exercise.Id,
                            media.MediaUrl,
                            media.MediaKind,
                            media.ThumbnailUrl,
                            media.SourcePageUrl,
                            media.SourceProvider ?? provider.ProviderName,
                            media.SourcePayloadJson,
                            media.SourceTitle,
                            Convert.ToDecimal(media.MatchScore));

                        await _candidateRepository.AddAsync(existingCandidate, cancellationToken);
                        exerciseCandidates.Add(existingCandidate);
                    }
                    else
                    {
                        existingCandidate.Refresh(
                            media.MediaUrl,
                            media.MediaKind,
                            media.ThumbnailUrl,
                            media.SourcePageUrl,
                            media.SourceProvider ?? provider.ProviderName,
                            media.SourcePayloadJson,
                            media.SourceTitle,
                            Convert.ToDecimal(media.MatchScore));
                    }
                }

                var bestCandidate = exerciseCandidates
                    .Where(candidate => !candidate.IsRejected)
                    .OrderByDescending(candidate => candidate.MatchScore)
                    .ThenByDescending(candidate => candidate.CreatedAt)
                    .FirstOrDefault();

                if (bestCandidate is not null
                    && (double)bestCandidate.MatchScore >= _options.MediaAutoAcceptScore
                    && CanAutoAccept(bestCandidate))
                {
                    foreach (var other in exerciseCandidates.Where(candidate => candidate.Id != bestCandidate.Id))
                    {
                        other.ClearSelection();
                    }

                    bestCandidate.AutoAccept(DateTime.UtcNow, "Automatically accepted by the media enrichment score threshold.");
                    exercise.ApplyMediaData(
                        bestCandidate.MediaUrl,
                        bestCandidate.MediaKind,
                        bestCandidate.ThumbnailUrl,
                        bestCandidate.SourcePageUrl,
                        bestCandidate.SourceProvider,
                        bestCandidate.SourcePayloadJson);
                    enriched++;
                }
            }

            _logger.LogInformation("Enriched media for {Count} exercises using {ProviderCount} configured media providers.", enriched, selectedProviders.Count);
            return enriched;
        }

        private static bool CanAutoAccept(ExerciseMediaCandidate candidate)
        {
            return AutoAcceptProviders.Contains(candidate.SourceProvider)
                && ExerciseMediaPolicy.IsSupportedExampleMedia(candidate.MediaKind, candidate.MediaUrl);
        }

        private IReadOnlyList<IExerciseMediaProvider> ResolveSelectedProviders()
        {
            var requested = _options.MediaProviders
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return _mediaProviders
                .Where(provider => provider.IsConfigured && (requested.Count == 0 || requested.Contains(provider.ProviderName)))
                .ToList()
                .AsReadOnly();
        }
    }
}

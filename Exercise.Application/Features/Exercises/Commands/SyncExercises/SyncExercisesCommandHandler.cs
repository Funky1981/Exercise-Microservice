using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Abstractions.Services;
using Exercise.Application.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;

namespace Exercise.Application.Features.Exercises.Commands.SyncExercises
{
    public class SyncExercisesCommandHandler : IRequestHandler<SyncExercisesCommand, SyncExercisesResult>
    {
        private const int SaveBatchSize = 250;

        private readonly IExerciseDataProvider _dataProvider;
        private readonly IExerciseMediaEnrichmentService _mediaEnrichmentService;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SyncExercisesCommandHandler> _logger;

        public SyncExercisesCommandHandler(
            IExerciseDataProvider dataProvider,
            IExerciseMediaEnrichmentService mediaEnrichmentService,
            IExerciseRepository exerciseRepository,
            IUnitOfWork unitOfWork,
            ILogger<SyncExercisesCommandHandler> logger)
        {
            _dataProvider = dataProvider;
            _mediaEnrichmentService = mediaEnrichmentService;
            _exerciseRepository = exerciseRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<SyncExercisesResult> Handle(SyncExercisesCommand request, CancellationToken cancellationToken)
        {
            // This sync is an operator-driven import job. Let it complete even if the HTTP client disconnects.
            var syncCancellationToken = CancellationToken.None;
            var remoteExercises = await _dataProvider.FetchExercisesAsync(syncCancellationToken);

            var existing = (await _exerciseRepository.GetAllForUpdateAsync(syncCancellationToken)).ToList();
            var byExternalId = existing
                .Where(e => !string.IsNullOrWhiteSpace(e.ExternalId))
                .GroupBy(e => e.ExternalId!.ToUpperInvariant())
                .ToDictionary(g => g.Key, g => g.First());
            var bySourceSignature = existing
                .Where(e => !string.IsNullOrWhiteSpace(e.SourceProvider))
                .GroupBy(e => BuildSourceSignature(e.SourceProvider, e.Name, e.BodyPart, e.TargetMuscle))
                .ToDictionary(g => g.Key, g => g.First());

            int added = 0;
            int updated = 0;
            int pendingChanges = 0;
            foreach (var remote in remoteExercises)
            {
                if (string.IsNullOrWhiteSpace(remote.Name)
                    || string.IsNullOrWhiteSpace(remote.BodyPart)
                    || string.IsNullOrWhiteSpace(remote.TargetMuscle))
                    continue;

                var secondaryMusclesJson = remote.SecondaryMuscles is { Count: > 0 }
                    ? JsonSerializer.Serialize(remote.SecondaryMuscles)
                    : null;
                var instructionsJson = remote.Instructions is { Count: > 0 }
                    ? JsonSerializer.Serialize(remote.Instructions)
                    : null;

                ExerciseEntity? exercise = null;
                if (!string.IsNullOrWhiteSpace(remote.ExternalId))
                {
                    byExternalId.TryGetValue(remote.ExternalId.ToUpperInvariant(), out exercise);
                }

                if (exercise is null && !string.IsNullOrWhiteSpace(remote.SourceProvider))
                {
                    bySourceSignature.TryGetValue(
                        BuildSourceSignature(remote.SourceProvider, remote.Name, remote.BodyPart, remote.TargetMuscle),
                        out exercise);
                }

                if (exercise is not null)
                {
                    var mergedEquipment = string.IsNullOrWhiteSpace(exercise.Equipment) ? remote.Equipment : exercise.Equipment;
                    var mergedGifUrl = string.IsNullOrWhiteSpace(exercise.GifUrl) ? remote.GifUrl : exercise.GifUrl;
                    var mergedExternalId = string.IsNullOrWhiteSpace(exercise.ExternalId) ? remote.ExternalId : exercise.ExternalId;
                    var mergedSourceProvider = string.IsNullOrWhiteSpace(exercise.SourceProvider) ? remote.SourceProvider : exercise.SourceProvider;
                    var mergedSecondaryMusclesJson = string.IsNullOrWhiteSpace(exercise.SecondaryMusclesJson) ? secondaryMusclesJson : exercise.SecondaryMusclesJson;
                    var mergedInstructionsJson = string.IsNullOrWhiteSpace(exercise.InstructionsJson) ? instructionsJson : exercise.InstructionsJson;
                    var mergedSourcePayloadJson = string.IsNullOrWhiteSpace(exercise.SourcePayloadJson) ? remote.SourcePayloadJson : exercise.SourcePayloadJson;
                    var mergedDescription = string.IsNullOrWhiteSpace(exercise.Description) ? remote.Description : exercise.Description;
                    var mergedDifficulty = string.IsNullOrWhiteSpace(exercise.Difficulty) ? remote.Difficulty : exercise.Difficulty;
                    var mergedCategory = string.IsNullOrWhiteSpace(exercise.Category) ? remote.Category : exercise.Category;
                    var currentMediaUrl = ExerciseMediaPolicy.IsSupportedExampleMedia(exercise.MediaKind, exercise.MediaUrl)
                        && ExerciseMediaPolicy.IsAuthoritativeProvider(exercise.MediaSourceProvider)
                        ? exercise.MediaUrl
                        : null;
                    var currentMediaKind = currentMediaUrl is not null ? exercise.MediaKind : null;
                    var currentMediaThumbnailUrl = currentMediaUrl is not null ? exercise.MediaThumbnailUrl : null;
                    var currentMediaSourcePageUrl = currentMediaUrl is not null ? exercise.MediaSourcePageUrl : null;
                    var currentMediaSourceProvider = currentMediaUrl is not null ? exercise.MediaSourceProvider : null;
                    var currentMediaSourcePayloadJson = currentMediaUrl is not null ? exercise.MediaSourcePayloadJson : null;

                    var remoteCandidateMediaUrl = ExerciseMediaPolicy.IsSupportedExampleMedia(remote.MediaKind, remote.MediaUrl)
                        ? remote.MediaUrl
                        : null;
                    var remoteCandidateMediaKind = remoteCandidateMediaUrl is not null ? remote.MediaKind : null;
                    var remoteCandidateMediaThumbnailUrl = InferThumbnailUrl(remoteCandidateMediaUrl, remoteCandidateMediaKind);
                    var remoteCandidateMediaSourcePageUrl = InferMediaSourcePageUrl(remoteCandidateMediaUrl);
                    var remoteCandidateMediaSourceProvider = InferMediaSourceProvider(remoteCandidateMediaUrl, remoteCandidateMediaSourcePageUrl) ?? remote.SourceProvider;
                    var remoteMediaUrl = remoteCandidateMediaUrl is not null && ExerciseMediaPolicy.IsAuthoritativeProvider(remoteCandidateMediaSourceProvider)
                        ? remoteCandidateMediaUrl
                        : null;
                    var remoteMediaKind = remoteMediaUrl is not null ? remoteCandidateMediaKind : null;
                    var remoteMediaThumbnailUrl = remoteMediaUrl is not null ? remoteCandidateMediaThumbnailUrl : null;
                    var remoteMediaSourcePageUrl = remoteMediaUrl is not null ? remoteCandidateMediaSourcePageUrl : null;
                    var remoteMediaSourceProvider = remoteMediaUrl is not null ? remoteCandidateMediaSourceProvider : null;

                    var preferRemoteMedia = remoteMediaUrl is not null
                        && (currentMediaUrl is null || ExerciseMediaPolicy.IsAuthoritativeProvider(remote.SourceProvider));

                    var mergedMediaUrl = preferRemoteMedia ? remoteMediaUrl : currentMediaUrl;
                    var mergedMediaKind = preferRemoteMedia ? remoteMediaKind : currentMediaKind;
                    var mergedMediaThumbnailUrl = preferRemoteMedia
                        ? remoteMediaThumbnailUrl
                        : InferThumbnailUrl(currentMediaUrl, currentMediaKind) ?? currentMediaThumbnailUrl;
                    var mergedMediaSourcePageUrl = preferRemoteMedia
                        ? remoteMediaSourcePageUrl
                        : InferMediaSourcePageUrl(currentMediaUrl) ?? currentMediaSourcePageUrl;
                    var mergedMediaSourceProvider = preferRemoteMedia
                        ? remoteMediaSourceProvider
                        : InferMediaSourceProvider(currentMediaUrl, currentMediaSourcePageUrl) ?? currentMediaSourceProvider;
                    var mergedMediaSourcePayloadJson = preferRemoteMedia ? null : currentMediaSourcePayloadJson;

                    exercise.ApplyExternalData(
                        remote.Name,
                        remote.BodyPart,
                        remote.TargetMuscle,
                        mergedEquipment,
                        mergedGifUrl,
                        mergedExternalId,
                        mergedSourceProvider,
                        mergedSecondaryMusclesJson,
                        mergedInstructionsJson,
                        mergedSourcePayloadJson,
                        mergedDescription,
                        mergedDifficulty,
                        mergedCategory,
                        mergedMediaUrl,
                        mergedMediaKind,
                        mergedMediaThumbnailUrl,
                        mergedMediaSourcePageUrl,
                        mergedMediaSourceProvider,
                        mergedMediaSourcePayloadJson);
                    updated++;
                    pendingChanges++;

                    if (pendingChanges >= SaveBatchSize)
                    {
                        await SaveBatchAsync(added, updated, pendingChanges, syncCancellationToken);
                        pendingChanges = 0;
                    }

                    continue;
                }

                var newExercise = new ExerciseEntity(
                    Guid.NewGuid(),
                    remote.Name,
                    remote.BodyPart,
                    remote.TargetMuscle,
                    equipment: remote.Equipment,
                    gifUrl: remote.GifUrl,
                    externalId: remote.ExternalId,
                    sourceProvider: remote.SourceProvider,
                    secondaryMusclesJson: secondaryMusclesJson,
                    instructionsJson: instructionsJson,
                    sourcePayloadJson: remote.SourcePayloadJson,
                    category: remote.Category,
                    mediaUrl: remote.MediaUrl,
                    mediaKind: remote.MediaKind);

                newExercise.ApplyExternalData(
                    newExercise.Name,
                    newExercise.BodyPart,
                    newExercise.TargetMuscle,
                    newExercise.Equipment,
                    newExercise.GifUrl,
                    newExercise.ExternalId,
                    newExercise.SourceProvider,
                    newExercise.SecondaryMusclesJson,
                    newExercise.InstructionsJson,
                    newExercise.SourcePayloadJson,
                    remote.Description,
                    remote.Difficulty,
                    remote.Category,
                    newExercise.MediaUrl,
                    newExercise.MediaKind,
                    newExercise.MediaThumbnailUrl,
                    newExercise.MediaSourcePageUrl,
                    newExercise.MediaSourceProvider,
                    newExercise.MediaSourcePayloadJson);

                await _exerciseRepository.AddAsync(newExercise, syncCancellationToken);
                existing.Add(newExercise);
                if (!string.IsNullOrWhiteSpace(remote.ExternalId))
                {
                    byExternalId[remote.ExternalId.ToUpperInvariant()] = newExercise;
                }
                if (!string.IsNullOrWhiteSpace(remote.SourceProvider))
                {
                    bySourceSignature[BuildSourceSignature(remote.SourceProvider, remote.Name, remote.BodyPart, remote.TargetMuscle)] = newExercise;
                }
                added++;
                pendingChanges++;

                if (pendingChanges >= SaveBatchSize)
                {
                    await SaveBatchAsync(added, updated, pendingChanges, syncCancellationToken);
                    pendingChanges = 0;
                }
            }

            if (pendingChanges > 0)
            {
                await SaveBatchAsync(added, updated, pendingChanges, syncCancellationToken);
            }

            var mediaEnriched = await _mediaEnrichmentService.EnrichMissingMediaAsync(
                existing,
                request.MediaExerciseLimit,
                syncCancellationToken);

            if (mediaEnriched > 0)
            {
                await _unitOfWork.SaveChangesAsync(syncCancellationToken);
            }

            _logger.LogInformation(
                "Exercise sync complete. {Added} added, {Updated} updated, {MediaEnriched} media-enriched out of {Total} fetched.",
                added,
                updated,
                mediaEnriched,
                remoteExercises.Count);

            return new SyncExercisesResult(added, updated, mediaEnriched, remoteExercises.Count);
        }

        private static string BuildSourceSignature(string? sourceProvider, string name, string bodyPart, string targetMuscle)
        {
            return string.Join('|',
                sourceProvider?.Trim().ToUpperInvariant() ?? string.Empty,
                name.Trim().ToUpperInvariant(),
                bodyPart.Trim().ToUpperInvariant(),
                targetMuscle.Trim().ToUpperInvariant());
        }

        private async Task SaveBatchAsync(int added, int updated, int batchSize, CancellationToken cancellationToken)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Persisted exercise sync batch of {BatchSize} changes. Running totals: {Added} added, {Updated} updated.",
                batchSize,
                added,
                updated);
        }

        private static string? InferMediaSourceProvider(string? mediaUrl, string? mediaSourcePageUrl)
        {
            var source = $"{mediaUrl} {mediaSourcePageUrl}".ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            if (source.Contains("wger.de/media/", StringComparison.Ordinal))
            {
                return "Wger";
            }

            if (source.Contains("pexels.com", StringComparison.Ordinal)
                || source.Contains("images.pexels.com", StringComparison.Ordinal)
                || source.Contains("videos.pexels.com", StringComparison.Ordinal))
            {
                return "Pexels";
            }

            if (source.Contains("commons.wikimedia.org", StringComparison.Ordinal)
                || source.Contains("upload.wikimedia.org", StringComparison.Ordinal))
            {
                return "WikimediaCommons";
            }

            if (source.Contains("api.openverse.org", StringComparison.Ordinal)
                || source.Contains("openverse.org", StringComparison.Ordinal))
            {
                return "Openverse";
            }

            return null;
        }

        private static string? InferMediaSourcePageUrl(string? mediaUrl)
        {
            if (string.IsNullOrWhiteSpace(mediaUrl))
            {
                return null;
            }

            if (mediaUrl.Contains("wger.de/media/", StringComparison.OrdinalIgnoreCase)
                || mediaUrl.Contains("images.pexels.com", StringComparison.OrdinalIgnoreCase)
                || mediaUrl.Contains("videos.pexels.com", StringComparison.OrdinalIgnoreCase))
            {
                return mediaUrl;
            }

            return null;
        }

        private static string? InferThumbnailUrl(string? mediaUrl, string? mediaKind)
        {
            if (string.IsNullOrWhiteSpace(mediaUrl)
                || !string.IsNullOrWhiteSpace(mediaKind) && mediaKind.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return mediaUrl;
        }
    }
}

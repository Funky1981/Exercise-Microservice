using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Abstractions.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;

namespace Exercise.Application.Features.Exercises.Commands.SyncExercises
{
    public class SyncExercisesCommandHandler : IRequestHandler<SyncExercisesCommand, SyncExercisesResult>
    {
        private readonly IExerciseDataProvider _dataProvider;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SyncExercisesCommandHandler> _logger;

        public SyncExercisesCommandHandler(
            IExerciseDataProvider dataProvider,
            IExerciseRepository exerciseRepository,
            IUnitOfWork unitOfWork,
            ILogger<SyncExercisesCommandHandler> logger)
        {
            _dataProvider = dataProvider;
            _exerciseRepository = exerciseRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<SyncExercisesResult> Handle(SyncExercisesCommand request, CancellationToken cancellationToken)
        {
            var remoteExercises = new List<ExternalExerciseDto>();
            var pageSize = Math.Max(1, request.Limit);
            var offset = Math.Max(0, request.Offset);

            for (var page = 0; page < 500; page++)
            {
                var batch = await _dataProvider.FetchExercisesAsync(pageSize, offset, cancellationToken);
                if (batch.Count == 0)
                {
                    break;
                }

                remoteExercises.AddRange(batch);
                offset += batch.Count;
            }

            var existing = await _exerciseRepository.GetAllForUpdateAsync(cancellationToken);
            var byExternalId = existing
                .Where(e => !string.IsNullOrWhiteSpace(e.ExternalId))
                .GroupBy(e => e.ExternalId!.ToUpperInvariant())
                .ToDictionary(g => g.Key, g => g.First());
            var byName = existing
                .GroupBy(e => e.Name.ToUpperInvariant())
                .ToDictionary(g => g.Key, g => g.First());

            int added = 0;
            int updated = 0;
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

                if (exercise is null)
                {
                    byName.TryGetValue(remote.Name.ToUpperInvariant(), out exercise);
                }

                if (exercise is not null)
                {
                    exercise.ApplyExternalData(
                        remote.Name,
                        remote.BodyPart,
                        remote.TargetMuscle,
                        string.IsNullOrWhiteSpace(remote.Equipment) ? exercise.Equipment : remote.Equipment,
                        string.IsNullOrWhiteSpace(remote.GifUrl) ? exercise.GifUrl : remote.GifUrl,
                        remote.ExternalId,
                        remote.SourceProvider,
                        secondaryMusclesJson ?? exercise.SecondaryMusclesJson,
                        instructionsJson ?? exercise.InstructionsJson,
                        remote.SourcePayloadJson ?? exercise.SourcePayloadJson,
                        string.IsNullOrWhiteSpace(remote.Description) ? exercise.Description : remote.Description,
                        string.IsNullOrWhiteSpace(remote.Difficulty) ? exercise.Difficulty : remote.Difficulty,
                        string.IsNullOrWhiteSpace(remote.Category) ? exercise.Category : remote.Category,
                        string.IsNullOrWhiteSpace(remote.MediaUrl) ? exercise.MediaUrl : remote.MediaUrl,
                        string.IsNullOrWhiteSpace(remote.MediaKind) ? exercise.MediaKind : remote.MediaKind);
                    updated++;
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

                newExercise.UpdateDescription(remote.Description);
                newExercise.Update(
                    newExercise.Name,
                    newExercise.BodyPart,
                    newExercise.TargetMuscle,
                    newExercise.Equipment,
                    newExercise.GifUrl,
                    remote.Description,
                    remote.Difficulty,
                    newExercise.ExternalId,
                    newExercise.SourceProvider,
                    newExercise.SecondaryMusclesJson,
                    newExercise.InstructionsJson,
                    newExercise.SourcePayloadJson,
                    remote.Category,
                    newExercise.MediaUrl,
                    newExercise.MediaKind);

                await _exerciseRepository.AddAsync(newExercise, cancellationToken);
                if (!string.IsNullOrWhiteSpace(remote.ExternalId))
                {
                    byExternalId[remote.ExternalId.ToUpperInvariant()] = newExercise;
                }
                byName[remote.Name.ToUpperInvariant()] = newExercise;
                added++;
            }

            if (added > 0 || updated > 0)
                await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Exercise sync complete. {Added} added, {Updated} updated out of {Total} fetched.",
                added, updated, remoteExercises.Count);

            return new SyncExercisesResult(added, updated, remoteExercises.Count);
        }
    }
}

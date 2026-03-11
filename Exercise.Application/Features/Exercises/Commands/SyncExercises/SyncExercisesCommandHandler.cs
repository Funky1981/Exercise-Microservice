using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Abstractions.Services;
using MediatR;
using Microsoft.Extensions.Logging;
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
            var remoteExercises = await _dataProvider.FetchExercisesAsync(
                request.Limit, request.Offset, cancellationToken);

            var existing = await _exerciseRepository.GetAllAsync(cancellationToken);
            var existingNames = existing
                .Select(e => e.Name.ToUpperInvariant())
                .ToHashSet();

            int added = 0;
            foreach (var remote in remoteExercises)
            {
                if (string.IsNullOrWhiteSpace(remote.Name)
                    || string.IsNullOrWhiteSpace(remote.BodyPart)
                    || string.IsNullOrWhiteSpace(remote.TargetMuscle))
                    continue;

                if (existingNames.Contains(remote.Name.ToUpperInvariant()))
                    continue;

                var exercise = new ExerciseEntity(
                    Guid.NewGuid(),
                    remote.Name,
                    remote.BodyPart,
                    remote.TargetMuscle,
                    equipment: remote.Equipment,
                    gifUrl: remote.GifUrl);

                await _exerciseRepository.AddAsync(exercise, cancellationToken);
                added++;
            }

            if (added > 0)
                await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Exercise sync complete. {Added} new exercises added out of {Total} fetched.",
                added, remoteExercises.Count);

            return new SyncExercisesResult(added, remoteExercises.Count);
        }
    }
}

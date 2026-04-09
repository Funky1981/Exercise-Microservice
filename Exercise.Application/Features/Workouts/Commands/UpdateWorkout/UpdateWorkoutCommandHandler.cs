using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using Exercise.Domain.Entities;
using MediatR;

namespace Exercise.Application.Features.Workouts.Commands.UpdateWorkout
{
    public class UpdateWorkoutCommandHandler : IRequestHandler<UpdateWorkoutCommand, bool>
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateWorkoutCommandHandler(
            IExerciseRepository exerciseRepository,
            IWorkoutRepository workoutRepository,
            IUnitOfWork unitOfWork)
        {
            _exerciseRepository = exerciseRepository;
            _workoutRepository = workoutRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateWorkoutCommand request, CancellationToken cancellationToken)
        {
            var workout = await _workoutRepository.GetOwnedByIdWithExercisesForUpdateAsync(
                request.WorkoutId, request.CurrentUserId, cancellationToken);
            if (workout is null)
            {
                if (await _workoutRepository.ExistsAsync(request.WorkoutId, cancellationToken))
                    throw new ForbiddenException("You do not have access to this workout.");

                throw new NotFoundException(nameof(Exercise.Domain.Entities.Workout), request.WorkoutId);
            }

            var exercises = await _exerciseRepository.GetByIdsAsync(request.ExerciseIds, cancellationToken);
            var missingExerciseId = request.ExerciseIds.FirstOrDefault(id => exercises.All(exercise => exercise.Id != id));
            if (missingExerciseId != Guid.Empty)
            {
                throw new NotFoundException(nameof(Exercise), missingExerciseId);
            }

            workout.ReplaceExercises(exercises);
            workout.Update(request.Name, request.Date, request.Notes, request.HasExplicitTime);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

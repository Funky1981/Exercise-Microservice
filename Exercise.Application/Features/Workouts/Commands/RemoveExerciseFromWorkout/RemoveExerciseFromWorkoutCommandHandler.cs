using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.Workouts.Commands.RemoveExerciseFromWorkout
{
    public class RemoveExerciseFromWorkoutCommandHandler : IRequestHandler<RemoveExerciseFromWorkoutCommand, bool>
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveExerciseFromWorkoutCommandHandler(
            IWorkoutRepository workoutRepository,
            IUnitOfWork unitOfWork)
        {
            _workoutRepository = workoutRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(RemoveExerciseFromWorkoutCommand request, CancellationToken cancellationToken)
        {
            var workout = await _workoutRepository.GetByIdWithExercisesAsync(request.WorkoutId, cancellationToken);
            if (workout is null)
                throw new NotFoundException(nameof(workout), request.WorkoutId);

            workout.RemoveExercise(request.ExerciseId);

            await _workoutRepository.UpdateAsync(workout, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

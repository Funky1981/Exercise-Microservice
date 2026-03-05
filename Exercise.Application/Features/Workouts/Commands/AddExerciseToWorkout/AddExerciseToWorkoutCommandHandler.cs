using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.Workouts.Commands.AddExerciseToWorkout
{
    public class AddExerciseToWorkoutCommandHandler : IRequestHandler<AddExerciseToWorkoutCommand, bool>
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddExerciseToWorkoutCommandHandler(
            IWorkoutRepository workoutRepository,
            IExerciseRepository exerciseRepository,
            IUnitOfWork unitOfWork)
        {
            _workoutRepository = workoutRepository;
            _exerciseRepository = exerciseRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(AddExerciseToWorkoutCommand request, CancellationToken cancellationToken)
        {
            var workout = await _workoutRepository.GetByIdWithExercisesAsync(request.WorkoutId, cancellationToken);
            if (workout is null)
                throw new NotFoundException(nameof(workout), request.WorkoutId);

            var exercise = await _exerciseRepository.GetByIdAsync(request.ExerciseId, cancellationToken);
            if (exercise is null)
                throw new NotFoundException(nameof(exercise), request.ExerciseId);

            workout.AddExercise(exercise);

            await _workoutRepository.UpdateAsync(workout, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

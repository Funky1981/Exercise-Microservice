using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using Exercise.Domain.Entities;
using MediatR;

namespace Exercise.Application.Features.Workouts.Commands.CreateWorkout
{
    public class CreateWorkoutCommandHandler : IRequestHandler<CreateWorkoutCommand, Guid>
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateWorkoutCommandHandler(
            IExerciseRepository exerciseRepository,
            IWorkoutRepository workoutRepository,
            IUnitOfWork unitOfWork)
        {
            _exerciseRepository = exerciseRepository;
            _workoutRepository = workoutRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateWorkoutCommand request, CancellationToken cancellationToken)
        {
            var exercises = await _exerciseRepository.GetByIdsAsync(request.ExerciseIds, cancellationToken);
            var missingExerciseId = request.ExerciseIds.FirstOrDefault(id => exercises.All(exercise => exercise.Id != id));
            if (missingExerciseId != Guid.Empty)
            {
                throw new NotFoundException(nameof(Exercise), missingExerciseId);
            }

            var workout = new Workout(
                Guid.NewGuid(),
                request.UserId,
                request.Name,
                request.Date,
                request.HasExplicitTime);
            workout.UpdateNotes(request.Notes);
            workout.ReplaceExercises(exercises);

            await _workoutRepository.AddAsync(workout, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return workout.Id;
        }
    }
}

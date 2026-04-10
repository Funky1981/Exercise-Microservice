using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.Workouts.Commands.UpdateExercisePrescription
{
    public class UpdateExercisePrescriptionCommandHandler : IRequestHandler<UpdateExercisePrescriptionCommand, bool>
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateExercisePrescriptionCommandHandler(
            IWorkoutRepository workoutRepository,
            IUnitOfWork unitOfWork)
        {
            _workoutRepository = workoutRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateExercisePrescriptionCommand request, CancellationToken cancellationToken)
        {
            var workout = await _workoutRepository.GetOwnedByIdWithExercisesForUpdateAsync(
                request.WorkoutId, request.CurrentUserId, cancellationToken);

            if (workout is null)
            {
                if (await _workoutRepository.ExistsAsync(request.WorkoutId, cancellationToken))
                    throw new ForbiddenException("You do not have access to this workout.");

                throw new NotFoundException(nameof(Domain.Entities.Workout), request.WorkoutId);
            }

            workout.UpdateExercisePrescription(request.ExerciseId, request.Sets, request.Reps, request.RestSeconds);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

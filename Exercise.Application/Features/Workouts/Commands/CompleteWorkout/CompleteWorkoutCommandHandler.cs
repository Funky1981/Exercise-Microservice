using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.Workouts.Commands.CompleteWorkout
{
    public class CompleteWorkoutCommandHandler : IRequestHandler<CompleteWorkoutCommand, bool>
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CompleteWorkoutCommandHandler(IWorkoutRepository workoutRepository, IUnitOfWork unitOfWork)
        {
            _workoutRepository = workoutRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(CompleteWorkoutCommand request, CancellationToken cancellationToken)
        {
            var workout = await _workoutRepository.GetOwnedByIdForUpdateAsync(
                request.WorkoutId, request.CurrentUserId, cancellationToken);
            if (workout is null)
            {
                if (await _workoutRepository.ExistsAsync(request.WorkoutId, cancellationToken))
                    throw new ForbiddenException("You do not have access to this workout.");

                throw new NotFoundException(nameof(Exercise.Domain.Entities.Workout), request.WorkoutId);
            }

            workout.CompleteWorkout(request.Duration);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

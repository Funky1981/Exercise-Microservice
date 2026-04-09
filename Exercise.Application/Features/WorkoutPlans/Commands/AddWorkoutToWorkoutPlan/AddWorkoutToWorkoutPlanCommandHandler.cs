using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.WorkoutPlans.Commands.AddWorkoutToWorkoutPlan
{
    public class AddWorkoutToWorkoutPlanCommandHandler : IRequestHandler<AddWorkoutToWorkoutPlanCommand, bool>
    {
        private readonly IWorkoutPlanRepository _workoutPlanRepository;
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddWorkoutToWorkoutPlanCommandHandler(
            IWorkoutPlanRepository workoutPlanRepository,
            IWorkoutRepository workoutRepository,
            IUnitOfWork unitOfWork)
        {
            _workoutPlanRepository = workoutPlanRepository;
            _workoutRepository = workoutRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(AddWorkoutToWorkoutPlanCommand request, CancellationToken cancellationToken)
        {
            var plan = await _workoutPlanRepository.GetOwnedByIdWithWorkoutsForUpdateAsync(
                request.WorkoutPlanId, request.CurrentUserId, cancellationToken);
            if (plan is null)
            {
                if (await _workoutPlanRepository.ExistsAsync(request.WorkoutPlanId, cancellationToken))
                    throw new ForbiddenException("You do not have access to this workout plan.");

                throw new NotFoundException(nameof(Exercise.Domain.Entities.WorkoutPlan), request.WorkoutPlanId);
            }

            var workout = await _workoutRepository.GetByIdForUpdateAsync(request.WorkoutId, cancellationToken);
            if (workout is null)
                throw new NotFoundException(nameof(workout), request.WorkoutId);

            plan.AddWorkout(workout);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

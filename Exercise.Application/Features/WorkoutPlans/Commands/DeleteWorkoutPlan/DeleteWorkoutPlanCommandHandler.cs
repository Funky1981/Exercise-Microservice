using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.WorkoutPlans.Commands.DeleteWorkoutPlan
{
    public class DeleteWorkoutPlanCommandHandler : IRequestHandler<DeleteWorkoutPlanCommand, bool>
    {
        private readonly IWorkoutPlanRepository _workoutPlanRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteWorkoutPlanCommandHandler(IWorkoutPlanRepository workoutPlanRepository, IUnitOfWork unitOfWork)
        {
            _workoutPlanRepository = workoutPlanRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DeleteWorkoutPlanCommand request, CancellationToken cancellationToken)
        {
            var plan = await _workoutPlanRepository.GetOwnedByIdForUpdateAsync(
                request.WorkoutPlanId, request.CurrentUserId, cancellationToken);
            if (plan is null)
            {
                if (await _workoutPlanRepository.ExistsAsync(request.WorkoutPlanId, cancellationToken))
                    throw new ForbiddenException("You do not have access to this workout plan.");

                throw new NotFoundException(nameof(Exercise.Domain.Entities.WorkoutPlan), request.WorkoutPlanId);
            }

            await _workoutPlanRepository.DeleteAsync(plan, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

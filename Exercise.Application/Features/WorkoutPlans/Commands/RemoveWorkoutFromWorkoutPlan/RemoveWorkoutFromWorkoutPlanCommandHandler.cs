using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.WorkoutPlans.Commands.RemoveWorkoutFromWorkoutPlan
{
    public class RemoveWorkoutFromWorkoutPlanCommandHandler : IRequestHandler<RemoveWorkoutFromWorkoutPlanCommand, bool>
    {
        private readonly IWorkoutPlanRepository _workoutPlanRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RemoveWorkoutFromWorkoutPlanCommandHandler(
            IWorkoutPlanRepository workoutPlanRepository,
            IUnitOfWork unitOfWork)
        {
            _workoutPlanRepository = workoutPlanRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(RemoveWorkoutFromWorkoutPlanCommand request, CancellationToken cancellationToken)
        {
            var plan = await _workoutPlanRepository.GetByIdWithWorkoutsAsync(request.WorkoutPlanId, cancellationToken);
            if (plan is null)
                throw new NotFoundException(nameof(plan), request.WorkoutPlanId);

            plan.RemoveWorkout(request.WorkoutId);

            await _workoutPlanRepository.UpdateAsync(plan, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

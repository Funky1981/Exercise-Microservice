using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using MediatR;

namespace Exercise.Application.Features.WorkoutPlans.Commands.UpdateWorkoutPlan
{
    public class UpdateWorkoutPlanCommandHandler : IRequestHandler<UpdateWorkoutPlanCommand, bool>
    {
        private readonly IWorkoutPlanRepository _workoutPlanRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateWorkoutPlanCommandHandler(IWorkoutPlanRepository workoutPlanRepository, IUnitOfWork unitOfWork)
        {
            _workoutPlanRepository = workoutPlanRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateWorkoutPlanCommand request, CancellationToken cancellationToken)
        {
            var plan = await _workoutPlanRepository.GetByIdAsync(request.WorkoutPlanId, cancellationToken);
            if (plan is null)
                throw new NotFoundException(nameof(plan), request.WorkoutPlanId);

            plan.Update(request.Name, request.StartDate, request.EndDate, request.Notes);

            await _workoutPlanRepository.UpdateAsync(plan, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

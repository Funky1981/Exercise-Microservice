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
            var plan = await _workoutPlanRepository.GetByIdAsync(request.WorkoutPlanId, cancellationToken);
            if (plan is null)
                throw new NotFoundException(nameof(Domain.Entities.WorkoutPlan), request.WorkoutPlanId);

            await _workoutPlanRepository.DeleteAsync(plan, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

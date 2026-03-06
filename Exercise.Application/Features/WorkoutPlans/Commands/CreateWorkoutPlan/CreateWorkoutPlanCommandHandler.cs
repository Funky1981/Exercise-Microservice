using Exercise.Application.Abstractions.Repositories;
using Exercise.Domain.Entities;
using MediatR;

namespace Exercise.Application.Features.WorkoutPlans.Commands.CreateWorkoutPlan
{
    public class CreateWorkoutPlanCommandHandler : IRequestHandler<CreateWorkoutPlanCommand, Guid>
    {
        private readonly IWorkoutPlanRepository _workoutPlanRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateWorkoutPlanCommandHandler(IWorkoutPlanRepository workoutPlanRepository, IUnitOfWork unitOfWork)
        {
            _workoutPlanRepository = workoutPlanRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateWorkoutPlanCommand request, CancellationToken cancellationToken)
        {
            var plan = new WorkoutPlan(
                Guid.NewGuid(),
                request.UserId,
                request.Name,
                request.StartDate,
                request.EndDate);

            plan.UpdateNotes(request.Notes);

            await _workoutPlanRepository.AddAsync(plan, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return plan.Id;
        }
    }
}

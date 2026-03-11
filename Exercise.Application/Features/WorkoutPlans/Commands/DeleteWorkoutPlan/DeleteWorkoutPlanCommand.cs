using MediatR;

namespace Exercise.Application.Features.WorkoutPlans.Commands.DeleteWorkoutPlan
{
    public class DeleteWorkoutPlanCommand : IRequest<bool>
    {
        public Guid WorkoutPlanId { get; set; }
        public Guid CurrentUserId { get; set; }
    }
}

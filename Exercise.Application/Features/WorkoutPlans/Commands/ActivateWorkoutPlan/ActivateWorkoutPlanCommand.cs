using MediatR;

namespace Exercise.Application.Features.WorkoutPlans.Commands.ActivateWorkoutPlan
{
    public class ActivateWorkoutPlanCommand : IRequest<bool>
    {
        public Guid WorkoutPlanId { get; set; }
        public Guid CurrentUserId { get; set; }
    }
}

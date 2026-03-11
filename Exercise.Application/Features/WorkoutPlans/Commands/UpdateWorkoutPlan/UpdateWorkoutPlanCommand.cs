using MediatR;

namespace Exercise.Application.Features.WorkoutPlans.Commands.UpdateWorkoutPlan
{
    public class UpdateWorkoutPlanCommand : IRequest<bool>
    {
        public Guid WorkoutPlanId { get; set; }
        public Guid CurrentUserId { get; set; }
        public string? Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
    }
}

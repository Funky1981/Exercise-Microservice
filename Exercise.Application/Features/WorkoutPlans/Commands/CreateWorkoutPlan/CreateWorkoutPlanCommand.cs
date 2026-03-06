using MediatR;

namespace Exercise.Application.Features.WorkoutPlans.Commands.CreateWorkoutPlan
{
    public class CreateWorkoutPlanCommand : IRequest<Guid>
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
    }
}

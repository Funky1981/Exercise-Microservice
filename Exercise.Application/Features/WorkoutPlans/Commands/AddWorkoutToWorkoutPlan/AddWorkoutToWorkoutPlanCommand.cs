using MediatR;

namespace Exercise.Application.Features.WorkoutPlans.Commands.AddWorkoutToWorkoutPlan
{
    public class AddWorkoutToWorkoutPlanCommand : IRequest<bool>
    {
        public Guid WorkoutPlanId { get; set; }
        public Guid WorkoutId { get; set; }

        public AddWorkoutToWorkoutPlanCommand(Guid workoutPlanId, Guid workoutId)
        {
            WorkoutPlanId = workoutPlanId;
            WorkoutId = workoutId;
        }
    }
}

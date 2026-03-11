using MediatR;

namespace Exercise.Application.Features.WorkoutPlans.Commands.RemoveWorkoutFromWorkoutPlan
{
    public class RemoveWorkoutFromWorkoutPlanCommand : IRequest<bool>
    {
        public Guid WorkoutPlanId { get; set; }
        public Guid CurrentUserId { get; set; }
        public Guid WorkoutId { get; set; }

        public RemoveWorkoutFromWorkoutPlanCommand(Guid workoutPlanId, Guid workoutId)
        {
            WorkoutPlanId = workoutPlanId;
            WorkoutId = workoutId;
        }
    }
}

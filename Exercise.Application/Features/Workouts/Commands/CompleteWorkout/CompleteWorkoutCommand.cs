using MediatR;

namespace Exercise.Application.Features.Workouts.Commands.CompleteWorkout
{
    public class CompleteWorkoutCommand : IRequest<bool>
    {
        public Guid WorkoutId { get; set; }
        public TimeSpan Duration { get; set; }

        public CompleteWorkoutCommand(Guid workoutId, TimeSpan duration)
        {
            WorkoutId = workoutId;
            Duration = duration;
        }
    }
}

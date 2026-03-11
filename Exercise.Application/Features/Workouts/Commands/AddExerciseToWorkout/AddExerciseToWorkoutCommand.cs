using MediatR;

namespace Exercise.Application.Features.Workouts.Commands.AddExerciseToWorkout
{
    public class AddExerciseToWorkoutCommand : IRequest<bool>
    {
        public Guid WorkoutId { get; set; }
        public Guid CurrentUserId { get; set; }
        public Guid ExerciseId { get; set; }

        public AddExerciseToWorkoutCommand(Guid workoutId, Guid exerciseId)
        {
            WorkoutId = workoutId;
            ExerciseId = exerciseId;
        }
    }
}

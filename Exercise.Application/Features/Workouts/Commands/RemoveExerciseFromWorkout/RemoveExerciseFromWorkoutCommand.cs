using MediatR;

namespace Exercise.Application.Features.Workouts.Commands.RemoveExerciseFromWorkout
{
    public class RemoveExerciseFromWorkoutCommand : IRequest<bool>
    {
        public Guid WorkoutId { get; set; }
        public Guid CurrentUserId { get; set; }
        public Guid ExerciseId { get; set; }

        public RemoveExerciseFromWorkoutCommand(Guid workoutId, Guid exerciseId)
        {
            WorkoutId = workoutId;
            ExerciseId = exerciseId;
        }
    }
}

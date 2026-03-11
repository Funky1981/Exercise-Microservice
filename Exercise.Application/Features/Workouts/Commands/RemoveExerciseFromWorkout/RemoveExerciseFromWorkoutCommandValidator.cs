using FluentValidation;

namespace Exercise.Application.Features.Workouts.Commands.RemoveExerciseFromWorkout
{
    public class RemoveExerciseFromWorkoutCommandValidator : AbstractValidator<RemoveExerciseFromWorkoutCommand>
    {
        public RemoveExerciseFromWorkoutCommandValidator()
        {
            RuleFor(x => x.WorkoutId).NotEmpty().WithMessage("WorkoutId is required.");
            RuleFor(x => x.CurrentUserId).NotEmpty().WithMessage("CurrentUserId is required.");
            RuleFor(x => x.ExerciseId).NotEmpty().WithMessage("ExerciseId is required.");
        }
    }
}

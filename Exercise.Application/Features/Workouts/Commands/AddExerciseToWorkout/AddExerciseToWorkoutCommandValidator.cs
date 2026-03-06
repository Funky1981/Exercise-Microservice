using FluentValidation;

namespace Exercise.Application.Features.Workouts.Commands.AddExerciseToWorkout
{
    public class AddExerciseToWorkoutCommandValidator : AbstractValidator<AddExerciseToWorkoutCommand>
    {
        public AddExerciseToWorkoutCommandValidator()
        {
            RuleFor(x => x.WorkoutId).NotEmpty().WithMessage("WorkoutId is required.");
            RuleFor(x => x.ExerciseId).NotEmpty().WithMessage("ExerciseId is required.");
        }
    }
}

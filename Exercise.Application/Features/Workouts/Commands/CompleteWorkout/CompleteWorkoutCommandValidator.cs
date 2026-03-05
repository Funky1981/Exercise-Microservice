using FluentValidation;

namespace Exercise.Application.Features.Workouts.Commands.CompleteWorkout
{
    public class CompleteWorkoutCommandValidator : AbstractValidator<CompleteWorkoutCommand>
    {
        public CompleteWorkoutCommandValidator()
        {
            RuleFor(x => x.WorkoutId).NotEmpty().WithMessage("WorkoutId is required.");
            RuleFor(x => x.Duration).GreaterThan(TimeSpan.Zero).WithMessage("Duration must be greater than zero.");
        }
    }
}

using FluentValidation;

namespace Exercise.Application.Features.ExerciseLogs.Commands.AddExerciseLogEntry
{
    public class AddExerciseLogEntryCommandValidator : AbstractValidator<AddExerciseLogEntryCommand>
    {
        public AddExerciseLogEntryCommandValidator()
        {
            RuleFor(x => x.LogId)
                .NotEmpty().WithMessage("LogId is required.");

            RuleFor(x => x.ExerciseId)
                .NotEmpty().WithMessage("ExerciseId is required.");

            RuleFor(x => x.Sets)
                .GreaterThan(0).WithMessage("Sets must be greater than 0.");

            RuleFor(x => x.Reps)
                .GreaterThan(0).WithMessage("Reps must be greater than 0.");

            RuleFor(x => x.Duration)
                .GreaterThan(TimeSpan.Zero).WithMessage("Duration must be positive.")
                .When(x => x.Duration.HasValue);
        }
    }
}

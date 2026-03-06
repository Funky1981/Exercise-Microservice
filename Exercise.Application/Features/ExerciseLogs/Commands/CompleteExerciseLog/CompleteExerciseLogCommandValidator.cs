using FluentValidation;

namespace Exercise.Application.Features.ExerciseLogs.Commands.CompleteExerciseLog
{
    public class CompleteExerciseLogCommandValidator : AbstractValidator<CompleteExerciseLogCommand>
    {
        public CompleteExerciseLogCommandValidator()
        {
            RuleFor(x => x.LogId)
                .NotEmpty().WithMessage("LogId is required.");

            RuleFor(x => x.TotalDuration)
                .GreaterThan(TimeSpan.Zero).WithMessage("TotalDuration must be positive.")
                .When(x => x.TotalDuration.HasValue);
        }
    }
}

using FluentValidation;

namespace Exercise.Application.Features.Sessions.Commands.LogSet
{
    public class LogSetCommandValidator : AbstractValidator<LogSetCommand>
    {
        public LogSetCommandValidator()
        {
            RuleFor(x => x.LogId).NotEmpty().WithMessage("LogId is required.");
            RuleFor(x => x.CurrentUserId).NotEmpty().WithMessage("UserId is required.");
            RuleFor(x => x.ExerciseId).NotEmpty().WithMessage("ExerciseId is required.");
            RuleFor(x => x.Reps).GreaterThanOrEqualTo(0).WithMessage("Reps cannot be negative.");
            RuleFor(x => x.DurationSeconds).GreaterThanOrEqualTo(0).WithMessage("Duration cannot be negative.");
            RuleFor(x => x.RestSeconds).GreaterThanOrEqualTo(0).WithMessage("RestSeconds cannot be negative.");
        }
    }
}

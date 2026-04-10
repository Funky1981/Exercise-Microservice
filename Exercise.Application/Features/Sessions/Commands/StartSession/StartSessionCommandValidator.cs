using FluentValidation;

namespace Exercise.Application.Features.Sessions.Commands.StartSession
{
    public class StartSessionCommandValidator : AbstractValidator<StartSessionCommand>
    {
        public StartSessionCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
            RuleFor(x => x.WorkoutId).NotEmpty().WithMessage("WorkoutId is required.");
        }
    }
}

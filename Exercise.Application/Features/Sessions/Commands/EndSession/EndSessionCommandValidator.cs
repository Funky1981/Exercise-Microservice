using FluentValidation;

namespace Exercise.Application.Features.Sessions.Commands.EndSession
{
    public class EndSessionCommandValidator : AbstractValidator<EndSessionCommand>
    {
        public EndSessionCommandValidator()
        {
            RuleFor(x => x.LogId).NotEmpty().WithMessage("LogId is required.");
            RuleFor(x => x.CurrentUserId).NotEmpty().WithMessage("UserId is required.");
            RuleFor(x => x.TotalDurationSeconds).GreaterThanOrEqualTo(0).WithMessage("TotalDurationSeconds cannot be negative.");
        }
    }
}

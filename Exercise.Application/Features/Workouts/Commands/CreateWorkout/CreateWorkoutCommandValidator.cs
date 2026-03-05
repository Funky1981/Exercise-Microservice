using FluentValidation;

namespace Exercise.Application.Features.Workouts.Commands.CreateWorkout
{
    public class CreateWorkoutCommandValidator : AbstractValidator<CreateWorkoutCommand>
    {
        public CreateWorkoutCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
            RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
            RuleFor(x => x.Name).MaximumLength(200).When(x => x.Name is not null);
            RuleFor(x => x.Notes).MaximumLength(1000).When(x => x.Notes is not null);
        }
    }
}

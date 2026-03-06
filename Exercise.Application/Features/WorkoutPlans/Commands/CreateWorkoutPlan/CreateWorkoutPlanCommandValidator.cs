using FluentValidation;

namespace Exercise.Application.Features.WorkoutPlans.Commands.CreateWorkoutPlan
{
    public class CreateWorkoutPlanCommandValidator : AbstractValidator<CreateWorkoutPlanCommand>
    {
        public CreateWorkoutPlanCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.Name)
                .MaximumLength(200).WithMessage("Name must not exceed 200 characters.")
                .When(x => x.Name is not null);

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("StartDate is required.");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .WithMessage("EndDate must be after StartDate.")
                .When(x => x.EndDate.HasValue);

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters.")
                .When(x => x.Notes is not null);
        }
    }
}

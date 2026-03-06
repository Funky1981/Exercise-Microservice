using FluentValidation;

namespace Exercise.Application.Features.WorkoutPlans.Commands.ActivateWorkoutPlan
{
    public class ActivateWorkoutPlanCommandValidator : AbstractValidator<ActivateWorkoutPlanCommand>
    {
        public ActivateWorkoutPlanCommandValidator()
        {
            RuleFor(x => x.WorkoutPlanId)
                .NotEmpty().WithMessage("WorkoutPlanId is required.");
        }
    }
}

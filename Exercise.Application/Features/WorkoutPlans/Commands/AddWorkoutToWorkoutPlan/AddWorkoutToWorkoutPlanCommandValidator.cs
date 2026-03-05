using FluentValidation;

namespace Exercise.Application.Features.WorkoutPlans.Commands.AddWorkoutToWorkoutPlan
{
    public class AddWorkoutToWorkoutPlanCommandValidator : AbstractValidator<AddWorkoutToWorkoutPlanCommand>
    {
        public AddWorkoutToWorkoutPlanCommandValidator()
        {
            RuleFor(x => x.WorkoutPlanId).NotEmpty().WithMessage("WorkoutPlanId is required.");
            RuleFor(x => x.WorkoutId).NotEmpty().WithMessage("WorkoutId is required.");
        }
    }
}

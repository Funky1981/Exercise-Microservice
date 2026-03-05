using FluentValidation;

namespace Exercise.Application.Features.WorkoutPlans.Commands.RemoveWorkoutFromWorkoutPlan
{
    public class RemoveWorkoutFromWorkoutPlanCommandValidator : AbstractValidator<RemoveWorkoutFromWorkoutPlanCommand>
    {
        public RemoveWorkoutFromWorkoutPlanCommandValidator()
        {
            RuleFor(x => x.WorkoutPlanId).NotEmpty().WithMessage("WorkoutPlanId is required.");
            RuleFor(x => x.WorkoutId).NotEmpty().WithMessage("WorkoutId is required.");
        }
    }
}

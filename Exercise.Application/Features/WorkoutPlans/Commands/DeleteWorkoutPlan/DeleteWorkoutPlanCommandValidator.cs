using FluentValidation;

namespace Exercise.Application.Features.WorkoutPlans.Commands.DeleteWorkoutPlan
{
    public class DeleteWorkoutPlanCommandValidator : AbstractValidator<DeleteWorkoutPlanCommand>
    {
        public DeleteWorkoutPlanCommandValidator()
        {
            RuleFor(x => x.WorkoutPlanId)
                .NotEmpty().WithMessage("WorkoutPlanId is required.");
        }
    }
}

using FluentValidation;

namespace Exercise.Application.Features.WorkoutPlans.Commands.UpdateWorkoutPlan
{
    public class UpdateWorkoutPlanCommandValidator : AbstractValidator<UpdateWorkoutPlanCommand>
    {
        public UpdateWorkoutPlanCommandValidator()
        {
            RuleFor(x => x.WorkoutPlanId).NotEmpty().WithMessage("WorkoutPlanId is required.");
            RuleFor(x => x.CurrentUserId).NotEmpty().WithMessage("CurrentUserId is required.");
            RuleFor(x => x.StartDate).NotEmpty().WithMessage("StartDate is required.");
        }
    }
}

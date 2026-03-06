using FluentValidation;

namespace Exercise.Application.Features.WorkoutPlans.Queries.GetWorkoutPlanById
{
    public class GetWorkoutPlanByIdQueryValidator : AbstractValidator<GetWorkoutPlanByIdQuery>
    {
        public GetWorkoutPlanByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("WorkoutPlan Id is required.");
        }
    }
}

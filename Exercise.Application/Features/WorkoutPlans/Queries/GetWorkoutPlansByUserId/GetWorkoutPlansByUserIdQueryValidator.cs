using FluentValidation;

namespace Exercise.Application.Features.WorkoutPlans.Queries.GetWorkoutPlansByUserId
{
    public class GetWorkoutPlansByUserIdQueryValidator : AbstractValidator<GetWorkoutPlansByUserIdQuery>
    {
        public GetWorkoutPlansByUserIdQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage("PageNumber must be at least 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");
        }
    }
}

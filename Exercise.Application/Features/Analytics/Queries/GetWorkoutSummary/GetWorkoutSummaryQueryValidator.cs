using FluentValidation;

namespace Exercise.Application.Features.Analytics.Queries.GetWorkoutSummary
{
    public class GetWorkoutSummaryQueryValidator : AbstractValidator<GetWorkoutSummaryQuery>
    {
        public GetWorkoutSummaryQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
        }
    }
}

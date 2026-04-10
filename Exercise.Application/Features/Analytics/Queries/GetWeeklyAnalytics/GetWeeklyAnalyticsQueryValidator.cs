using FluentValidation;

namespace Exercise.Application.Features.Analytics.Queries.GetWeeklyAnalytics
{
    public class GetWeeklyAnalyticsQueryValidator : AbstractValidator<GetWeeklyAnalyticsQuery>
    {
        public GetWeeklyAnalyticsQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Weeks).InclusiveBetween(1, 52).WithMessage("Weeks must be between 1 and 52.");
        }
    }
}

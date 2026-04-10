using FluentValidation;

namespace Exercise.Application.Features.Analytics.Queries.GetExerciseAnalytics
{
    public class GetExerciseAnalyticsQueryValidator : AbstractValidator<GetExerciseAnalyticsQuery>
    {
        public GetExerciseAnalyticsQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.ExerciseId).NotEmpty();
        }
    }
}

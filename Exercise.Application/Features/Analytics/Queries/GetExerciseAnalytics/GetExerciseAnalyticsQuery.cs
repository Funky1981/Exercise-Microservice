using Exercise.Application.Features.Analytics.Dtos;
using MediatR;

namespace Exercise.Application.Features.Analytics.Queries.GetExerciseAnalytics
{
    public class GetExerciseAnalyticsQuery : IRequest<ExerciseAnalyticsDto>
    {
        public Guid UserId { get; set; }
        public Guid ExerciseId { get; set; }
    }
}

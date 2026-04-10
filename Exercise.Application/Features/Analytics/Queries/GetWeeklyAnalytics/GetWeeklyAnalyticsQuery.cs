using Exercise.Application.Features.Analytics.Dtos;
using MediatR;

namespace Exercise.Application.Features.Analytics.Queries.GetWeeklyAnalytics
{
    public class GetWeeklyAnalyticsQuery : IRequest<WeeklyAnalyticsDto>
    {
        public Guid UserId { get; set; }
        /// <summary>Number of weeks to look back from today. Default 12.</summary>
        public int Weeks { get; set; } = 12;
    }
}

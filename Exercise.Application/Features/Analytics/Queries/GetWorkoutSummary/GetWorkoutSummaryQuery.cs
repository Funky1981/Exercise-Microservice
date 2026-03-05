using Exercise.Application.Features.Analytics.Dtos;
using MediatR;

namespace Exercise.Application.Features.Analytics.Queries.GetWorkoutSummary
{
    public class GetWorkoutSummaryQuery : IRequest<WorkoutSummaryDto>
    {
        public Guid UserId { get; set; }
    }
}

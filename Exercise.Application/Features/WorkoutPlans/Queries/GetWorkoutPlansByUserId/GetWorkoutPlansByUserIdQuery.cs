using Exercise.Application.Common.Models;
using Exercise.Application.Features.WorkoutPlans.Dtos;
using MediatR;

namespace Exercise.Application.Features.WorkoutPlans.Queries.GetWorkoutPlansByUserId
{
    public class GetWorkoutPlansByUserIdQuery : IRequest<PagedResult<WorkoutPlanDto>>
    {
        public Guid UserId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

using Exercise.Application.Common.Models;
using Exercise.Application.Features.Workouts.Dtos;
using MediatR;

namespace Exercise.Application.Features.Workouts.Queries.GetWorkoutsByUserId
{
    public class GetWorkoutsByUserIdQuery : IRequest<PagedResult<WorkoutDto>>
    {
        public Guid UserId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public GetWorkoutsByUserIdQuery(Guid userId, int pageNumber = 1, int pageSize = 20)
        {
            UserId = userId;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}

using Exercise.Application.Common.Models;
using Exercise.Application.Features.ExerciseLogs.Dtos;
using MediatR;

namespace Exercise.Application.Features.ExerciseLogs.Queries.GetExerciseLogsByUserId
{
    public class GetExerciseLogsByUserIdQuery : IRequest<PagedResult<ExerciseLogDto>>
    {
        public Guid UserId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

using Exercise.Application.Common.Models;
using Exercise.Application.Exercises.Dtos;
using MediatR;

namespace Exercise.Application.Features.Exercises.Queries.GetAllExercises
{
    public class GetAllExercisesQuery : IRequest<PagedResult<ExerciseDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize   { get; set; } = 20;

        public GetAllExercisesQuery(int pageNumber = 1, int pageSize = 20)
        {
            PageNumber = pageNumber;
            PageSize   = pageSize;
        }
    }
}
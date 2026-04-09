using Exercise.Application.Exercises.Dtos;
using MediatR;

namespace Exercise.Application.Features.Exercises.Queries.GetExerciseFilters
{
    public class GetExerciseFiltersQuery : IRequest<ExerciseFiltersDto>
    {
    }
}

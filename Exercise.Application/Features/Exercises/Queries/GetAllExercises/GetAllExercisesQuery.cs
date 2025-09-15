using Exercise.Application.Exercises.Dtos;
using MediatR;

namespace Exercise.Application.Features.Exercises.Queries.GetAllExercises
{
    public class GetAllExercisesQuery : IRequest<IReadOnlyList<ExerciseDto>>
    {
        
    }
}
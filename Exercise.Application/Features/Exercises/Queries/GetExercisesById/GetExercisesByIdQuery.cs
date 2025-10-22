using Exercise.Application.Exercises.Dtos;
using MediatR;

namespace Exercise.Application.Features.Exercises.Queries.GetExercisesById
{
    public class GetExercisesByIdQuery : IRequest<ExerciseDto>
    {
        public Guid Id { get; private set; }

        public GetExercisesByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}


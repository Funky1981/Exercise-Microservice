using Exercise.Application.Exercises.Dtos;
using MediatR;

namespace Exercise.Application.Features.Exercises.Queries.GetExercisesByBodyPart
{
    public class GetExercisesByBodyPartQuery : IRequest<IReadOnlyList<ExerciseDto>>
    {
        public string BodyPart { get; set; } = string.Empty;
    }
}
using Exercise.Application.Features.ExerciseLogs.Dtos;
using MediatR;

namespace Exercise.Application.Features.ExerciseLogs.Queries.GetExerciseLogById
{
    public class GetExerciseLogByIdQuery : IRequest<ExerciseLogDto?>
    {
        public Guid Id { get; set; }
    }
}

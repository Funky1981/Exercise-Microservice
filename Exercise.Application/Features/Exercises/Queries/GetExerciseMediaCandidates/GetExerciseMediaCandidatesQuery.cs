using Exercise.Application.Exercises.Dtos;
using MediatR;

namespace Exercise.Application.Features.Exercises.Queries.GetExerciseMediaCandidates
{
    public sealed record GetExerciseMediaCandidatesQuery(Guid ExerciseId) : IRequest<IReadOnlyList<ExerciseMediaCandidateDto>>;
}
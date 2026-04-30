using Exercise.Application.Exercises.Dtos;
using MediatR;

namespace Exercise.Application.Features.Exercises.Queries.GetPendingExerciseMediaCandidates
{
    public sealed record GetPendingExerciseMediaCandidatesQuery(int Limit = 100) : IRequest<IReadOnlyList<ExerciseMediaReviewQueueItemDto>>;
}
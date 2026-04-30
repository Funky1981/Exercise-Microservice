using MediatR;

namespace Exercise.Application.Features.Exercises.Commands.ReviewExerciseMediaCandidate
{
    public sealed record ReviewExerciseMediaCandidateCommand(
        Guid ExerciseId,
        Guid CandidateId,
        bool Approve,
        string? Notes = null) : IRequest;
}
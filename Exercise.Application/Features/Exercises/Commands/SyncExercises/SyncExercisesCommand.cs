using MediatR;

namespace Exercise.Application.Features.Exercises.Commands.SyncExercises
{
    /// <summary>
    /// Syncs exercises from an external provider into the local catalogue.
    /// Returns the number of newly added exercises.
    /// </summary>
    public record SyncExercisesCommand(int Limit = 100, int Offset = 0) : IRequest<SyncExercisesResult>;

    public record SyncExercisesResult(int Added, int Updated, int TotalFetched);
}

using MediatR;

namespace Exercise.Application.Features.Exercises.Commands.SyncExercises
{
    /// <summary>
    /// Syncs exercises from an external provider into the local catalogue.
    /// Returns the number of newly added exercises.
    /// </summary>
    public record SyncExercisesCommand(int? MediaExerciseLimit = null) : IRequest<SyncExercisesResult>;

    public record SyncExercisesResult(int Added, int Updated, int MediaEnriched, int TotalFetched);
}

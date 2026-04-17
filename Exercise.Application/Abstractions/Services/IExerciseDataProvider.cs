namespace Exercise.Application.Abstractions.Services
{
    /// <summary>
    /// Abstraction for fetching exercise data from an external API provider.
    /// Implement this interface to integrate with different exercise databases
    /// (e.g. RapidAPI ExerciseDB, wger, custom APIs).
    /// </summary>
    public interface IExerciseDataProvider
    {
        /// <summary>
        /// Fetches exercises from the external provider.
        /// </summary>
        /// <param name="limit">Maximum number of exercises to fetch.</param>
        /// <param name="offset">Offset for pagination.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A list of exercises from the external source.</returns>
        Task<IReadOnlyList<ExternalExerciseDto>> FetchExercisesAsync(
            int limit, int offset, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Provider-agnostic DTO representing an exercise from any external source.
    /// </summary>
    public record ExternalExerciseDto(
        string? ExternalId,
        string Name,
        string BodyPart,
        string TargetMuscle,
        string? Equipment = null,
        string? GifUrl = null,
        IReadOnlyList<string>? SecondaryMuscles = null,
        IReadOnlyList<string>? Instructions = null,
        string? SourcePayloadJson = null,
        string? SourceProvider = null,
        string? Description = null,
        string? Difficulty = null,
        string? Category = null);
}

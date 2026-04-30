using ExerciseEntity = Exercise.Domain.Entities.Exercise;

namespace Exercise.Application.Abstractions.Services
{
    /// <summary>
    /// Abstraction for fetching exercise data from an external API provider.
    /// Implement this interface to integrate with different exercise databases
    /// (e.g. RapidAPI ExerciseDB, wger, custom APIs).
    /// </summary>
    public interface IExerciseDataProvider
    {
        Task<IReadOnlyList<ExternalExerciseDto>> FetchExercisesAsync(CancellationToken cancellationToken = default);
    }

    public interface IExerciseCatalogProvider
    {
        string ProviderName { get; }

        Task<IReadOnlyList<ExternalExerciseDto>> FetchExercisesAsync(CancellationToken cancellationToken = default);
    }

    public interface IExerciseMediaProvider
    {
        string ProviderName { get; }

        bool IsConfigured { get; }

        Task<ExternalExerciseMediaDto?> FindMediaAsync(
            ExerciseMediaSearchQuery query,
            CancellationToken cancellationToken = default);
    }

    public interface IExerciseMediaEnrichmentService
    {
        Task<int> EnrichMissingMediaAsync(
            IReadOnlyCollection<ExerciseEntity> exercises,
            int? limit = null,
            CancellationToken cancellationToken = default);
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
        string? MediaUrl = null,
        string? MediaKind = null,
        IReadOnlyList<string>? SecondaryMuscles = null,
        IReadOnlyList<string>? Instructions = null,
        string? SourcePayloadJson = null,
        string? SourceProvider = null,
        string? Description = null,
        string? Difficulty = null,
        string? Category = null);

    public record ExerciseMediaSearchQuery(
        string Name,
        string? BodyPart = null,
        string? TargetMuscle = null,
        string? Equipment = null,
        string? ExternalId = null,
        string? SourceProvider = null,
        string? SourcePayloadJson = null);

    public record ExternalExerciseMediaDto(
        string? MediaUrl,
        string? MediaKind,
        string? ThumbnailUrl = null,
        string? SourcePageUrl = null,
        string? SourceProvider = null,
        string? SourcePayloadJson = null,
        string? SourceTitle = null,
        double MatchScore = 0d);
}

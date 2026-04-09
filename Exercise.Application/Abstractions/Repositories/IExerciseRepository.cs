using ExerciseEntity = Exercise.Domain.Entities.Exercise;

namespace Exercise.Application.Abstractions.Repositories
{
    /// <summary>
    /// Repository interface for managing Exercise entities
    /// </summary>
    public interface IExerciseRepository
    {
        /// <summary>
        /// Retrieves all exercises filtered by the specified body part
        /// </summary>
        /// <param name="bodyPart">The body part to filter exercises by</param>
        /// <param name="cancellationToken">A cancellation token for the operation</param>
        /// <returns>A read-only list of exercises that match the specified body part</returns>
        Task<IReadOnlyList<ExerciseEntity>> GetByBodyPartAsync(string bodyPart, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all exercises from the repository
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for the operation</param>
        /// <returns>A read-only list of all exercises</returns>
        Task<IReadOnlyList<ExerciseEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a paged slice of the exercise catalogue.
        /// </summary>
        Task<(IReadOnlyList<ExerciseEntity> Items, int TotalCount)> GetPagedAsync(
            int skip,
            int pageSize,
            string? search = null,
            string? bodyPart = null,
            string? equipment = null,
            IReadOnlyCollection<string>? regionBodyParts = null,
            bool otherRegionOnly = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a set of exercises by identifier, preserving the requested order where possible.
        /// </summary>
        Task<IReadOnlyList<ExerciseEntity>> GetByIdsAsync(
            IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a specific exercise by its unique identifier  
        /// </summary>
        /// <param name="id">The unique identifier of the exercise</param>
        /// <param name="cancellationToken">A cancellation token for the operation</param>
        /// <returns>The exercise that matches the specified identifier, or null if not found</returns>
        Task<ExerciseEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a specific exercise as a tracked entity for command-side updates.
        /// </summary>
        Task<ExerciseEntity?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns true when an exercise exists with the specified identifier.
        /// </summary>
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new exercise to the repository
        /// </summary>
        Task AddAsync(ExerciseEntity exercise, CancellationToken cancellationToken = default);

        /// <summary>
        /// Marks an existing exercise as modified
        /// </summary>
        Task UpdateAsync(ExerciseEntity exercise, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes an exercise from the repository
        /// </summary>
        Task DeleteAsync(ExerciseEntity exercise, CancellationToken cancellationToken = default);
    }
}

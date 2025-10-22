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
        /// Retrieves a specific exercise by its unique identifier  
        /// </summary>
        /// <param name="id">The unique identifier of the exercise</param>
        /// <param name="cancellationToken">A cancellation token for the operation</param>
        /// <returns>The exercise that matches the specified identifier, or null if not found</returns>
        Task<ExerciseEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
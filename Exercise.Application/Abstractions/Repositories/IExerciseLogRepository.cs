using Exercise.Domain.Entities;

namespace Exercise.Application.Abstractions.Repositories
{
    public interface IExerciseLogRepository
    {
        Task<ExerciseLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ExerciseLog?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ExerciseLog?> GetOwnedByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
        Task<ExerciseLog?> GetOwnedByIdForUpdateAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ExerciseLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<ExerciseLog> Items, int TotalCount)> GetPagedByUserIdAsync(Guid userId, int skip, int take, CancellationToken cancellationToken = default);
        Task<(int TotalCount, int CompletedCount, TimeSpan TotalDuration)> GetSummaryByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task AddAsync(ExerciseLog exerciseLog, CancellationToken cancellationToken = default);
        Task UpdateAsync(ExerciseLog exerciseLog, CancellationToken cancellationToken = default);
        Task DeleteAsync(ExerciseLog exerciseLog, CancellationToken cancellationToken = default);
    }
}

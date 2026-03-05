using Exercise.Domain.Entities;

namespace Exercise.Application.Abstractions.Repositories
{
    public interface IExerciseLogRepository
    {
        Task<ExerciseLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ExerciseLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<ExerciseLog> Items, int TotalCount)> GetPagedByUserIdAsync(Guid userId, int skip, int take, CancellationToken cancellationToken = default);
        Task AddAsync(ExerciseLog exerciseLog, CancellationToken cancellationToken = default);
        Task UpdateAsync(ExerciseLog exerciseLog, CancellationToken cancellationToken = default);
    }
}

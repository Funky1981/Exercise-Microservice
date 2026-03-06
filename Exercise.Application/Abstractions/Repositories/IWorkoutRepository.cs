using Exercise.Domain.Entities;

namespace Exercise.Application.Abstractions.Repositories
{
    public interface IWorkoutRepository
    {
        Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Workout?> GetByIdWithExercisesAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Workout>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<Workout> Items, int TotalCount)> GetPagedByUserIdAsync(Guid userId, int skip, int take, CancellationToken cancellationToken = default);
        Task AddAsync(Workout workout, CancellationToken cancellationToken = default);
        Task UpdateAsync(Workout workout, CancellationToken cancellationToken = default);
        Task DeleteAsync(Workout workout, CancellationToken cancellationToken = default);
    }
}

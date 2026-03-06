using Exercise.Domain.Entities;

namespace Exercise.Application.Abstractions.Repositories
{
    public interface IWorkoutPlanRepository
    {
        Task<WorkoutPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<WorkoutPlan?> GetByIdWithWorkoutsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<WorkoutPlan>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<WorkoutPlan> Items, int TotalCount)> GetPagedByUserIdAsync(Guid userId, int skip, int take, CancellationToken cancellationToken = default);
        Task AddAsync(WorkoutPlan workoutPlan, CancellationToken cancellationToken = default);
        Task UpdateAsync(WorkoutPlan workoutPlan, CancellationToken cancellationToken = default);
        Task DeleteAsync(WorkoutPlan workoutPlan, CancellationToken cancellationToken = default);
    }
}

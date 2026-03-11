using Exercise.Application.Abstractions.Repositories;
using Exercise.Domain.Entities;
using Exercise.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Exercise.Infrastructure.Repositories
{
    public class WorkoutPlanRepository : IWorkoutPlanRepository
    {
        private readonly ExerciseDbContext _context;

        public WorkoutPlanRepository(ExerciseDbContext context)
        {
            _context = context;
        }

        public async Task<WorkoutPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(wp => wp.Id == id, cancellationToken);
        }

        public async Task<WorkoutPlan?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutPlans
                .FirstOrDefaultAsync(wp => wp.Id == id, cancellationToken);
        }

        public async Task<WorkoutPlan?> GetByIdWithWorkoutsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutPlans
                .AsNoTracking()
                .Include(wp => wp.Workouts)
                .FirstOrDefaultAsync(wp => wp.Id == id, cancellationToken);
        }

        public async Task<WorkoutPlan?> GetByIdWithWorkoutsForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutPlans
                .Include(wp => wp.Workouts)
                .FirstOrDefaultAsync(wp => wp.Id == id, cancellationToken);
        }

        public async Task<WorkoutPlan?> GetOwnedByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutPlans
                .AsNoTracking()
                .FirstOrDefaultAsync(wp => wp.Id == id && wp.UserId == userId, cancellationToken);
        }

        public async Task<WorkoutPlan?> GetOwnedByIdForUpdateAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutPlans
                .FirstOrDefaultAsync(wp => wp.Id == id && wp.UserId == userId, cancellationToken);
        }

        public async Task<WorkoutPlan?> GetOwnedByIdWithWorkoutsForUpdateAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutPlans
                .Include(wp => wp.Workouts)
                .FirstOrDefaultAsync(wp => wp.Id == id && wp.UserId == userId, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutPlans.AnyAsync(wp => wp.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<WorkoutPlan>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutPlans
                .AsNoTracking()
                .Where(wp => wp.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IReadOnlyList<WorkoutPlan> Items, int TotalCount)> GetPagedByUserIdAsync(
            Guid userId, int skip, int take, CancellationToken cancellationToken = default)
        {
            var query = _context.WorkoutPlans
                .AsNoTracking()
                .Where(wp => wp.UserId == userId)
                .OrderByDescending(wp => wp.StartDate);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
            return (items, totalCount);
        }

        public async Task<(int TotalCount, int CompletedCount, TimeSpan TotalDuration)> GetSummaryByUserIdAsync(
            Guid userId, CancellationToken cancellationToken = default)
        {
            var summary = await _context.WorkoutPlans
                .AsNoTracking()
                .Where(wp => wp.UserId == userId)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalCount = g.Count(),
                    CompletedCount = g.Count(wp => wp.IsActive)
                })
                .SingleOrDefaultAsync(cancellationToken);

            return summary is null
                ? (0, 0, TimeSpan.Zero)
                : (summary.TotalCount, summary.CompletedCount, TimeSpan.Zero);
        }

        public async Task AddAsync(WorkoutPlan workoutPlan, CancellationToken cancellationToken = default)
        {
            await _context.WorkoutPlans.AddAsync(workoutPlan, cancellationToken);
        }

        public Task UpdateAsync(WorkoutPlan workoutPlan, CancellationToken cancellationToken = default)
        {
            _context.WorkoutPlans.Update(workoutPlan);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(WorkoutPlan workoutPlan, CancellationToken cancellationToken = default)
        {
            _context.WorkoutPlans.Remove(workoutPlan);
            return Task.CompletedTask;
        }
    }
}

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

        public async Task<IReadOnlyList<WorkoutPlan>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutPlans
                .AsNoTracking()
                .Where(wp => wp.UserId == userId)
                .ToListAsync(cancellationToken);
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

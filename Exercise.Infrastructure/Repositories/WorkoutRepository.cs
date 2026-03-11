using Exercise.Application.Abstractions.Repositories;
using Exercise.Domain.Entities;
using Exercise.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Exercise.Infrastructure.Repositories
{
    public class WorkoutRepository : IWorkoutRepository
    {
        private readonly ExerciseDbContext _context;

        public WorkoutRepository(ExerciseDbContext context)
        {
            _context = context;
        }

        public async Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Workouts
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }

        public async Task<Workout?> GetByIdWithExercisesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Workouts
                .Include(w => w.Exercises)
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Workout>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Workouts
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IReadOnlyList<Workout> Items, int TotalCount)> GetPagedByUserIdAsync(
            Guid userId, int skip, int take, CancellationToken cancellationToken = default)
        {
            var query = _context.Workouts
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.Date);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
            return (items, totalCount);
        }

        public async Task AddAsync(Workout workout, CancellationToken cancellationToken = default)
        {
            await _context.Workouts.AddAsync(workout, cancellationToken);
        }

        public Task UpdateAsync(Workout workout, CancellationToken cancellationToken = default)
        {
            _context.Workouts.Update(workout);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Workout workout, CancellationToken cancellationToken = default)
        {
            _context.Workouts.Remove(workout);
            return Task.CompletedTask;
        }
    }
}

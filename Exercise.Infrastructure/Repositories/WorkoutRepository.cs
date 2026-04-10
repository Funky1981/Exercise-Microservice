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

        public async Task<Workout?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Workouts
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }

        public async Task<Workout?> GetByIdWithExercisesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Workouts
                .AsNoTracking()
                .Include(w => w.WorkoutExercises).ThenInclude(we => we.Exercise)
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }

        public async Task<Workout?> GetByIdWithExercisesForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Workouts
                .Include(w => w.WorkoutExercises).ThenInclude(we => we.Exercise)
                .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
        }

        public async Task<Workout?> GetOwnedByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Workouts
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId, cancellationToken);
        }

        public async Task<Workout?> GetOwnedByIdWithExercisesAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Workouts
                .AsNoTracking()
                .Include(w => w.WorkoutExercises).ThenInclude(we => we.Exercise)
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId, cancellationToken);
        }

        public async Task<Workout?> GetOwnedByIdForUpdateAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Workouts
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId, cancellationToken);
        }

        public async Task<Workout?> GetOwnedByIdWithExercisesForUpdateAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Workouts
                .Include(w => w.WorkoutExercises).ThenInclude(we => we.Exercise)
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Workouts.AnyAsync(w => w.Id == id, cancellationToken);
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
                .Include(w => w.WorkoutExercises).ThenInclude(we => we.Exercise)
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.Date);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
            return (items, totalCount);
        }

        public async Task<(int TotalCount, int CompletedCount, TimeSpan TotalDuration)> GetSummaryByUserIdAsync(
            Guid userId, CancellationToken cancellationToken = default)
        {
            var counts = await _context.Workouts
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalCount = g.Count(),
                    CompletedCount = g.Count(w => w.IsCompleted)
                })
                .SingleOrDefaultAsync(cancellationToken);

            var totalDurationTicks = (await _context.Workouts
                    .AsNoTracking()
                    .Where(w => w.UserId == userId && w.IsCompleted && w.Duration.HasValue)
                    .Select(w => w.Duration!.Value)
                    .ToListAsync(cancellationToken))
                .Sum(duration => duration.Ticks);

            return counts is null
                ? (0, 0, TimeSpan.Zero)
                : (counts.TotalCount, counts.CompletedCount, TimeSpan.FromTicks(totalDurationTicks));
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

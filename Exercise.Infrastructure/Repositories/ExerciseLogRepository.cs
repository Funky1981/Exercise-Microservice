using Exercise.Application.Abstractions.Repositories;
using Exercise.Domain.Entities;
using Exercise.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Exercise.Infrastructure.Repositories
{
    public class ExerciseLogRepository : IExerciseLogRepository
    {
        private readonly ExerciseDbContext _context;

        public ExerciseLogRepository(ExerciseDbContext context)
        {
            _context = context;
        }

        public async Task<ExerciseLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ExerciseLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(el => el.Id == id, cancellationToken);
        }

        public async Task<ExerciseLog?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ExerciseLogs
                .FirstOrDefaultAsync(el => el.Id == id, cancellationToken);
        }

        public async Task<ExerciseLog?> GetOwnedByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.ExerciseLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(el => el.Id == id && el.UserId == userId, cancellationToken);
        }

        public async Task<ExerciseLog?> GetOwnedByIdForUpdateAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.ExerciseLogs
                .FirstOrDefaultAsync(el => el.Id == id && el.UserId == userId, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ExerciseLogs.AnyAsync(el => el.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<ExerciseLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.ExerciseLogs
                .AsNoTracking()
                .Where(el => el.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IReadOnlyList<ExerciseLog> Items, int TotalCount)> GetPagedByUserIdAsync(
            Guid userId, int skip, int take, CancellationToken cancellationToken = default)
        {
            var query = _context.ExerciseLogs
                .AsNoTracking()
                .Where(el => el.UserId == userId)
                .OrderByDescending(el => el.Date);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
            return (items, totalCount);
        }

        public async Task<(int TotalCount, int CompletedCount, TimeSpan TotalDuration)> GetSummaryByUserIdAsync(
            Guid userId, CancellationToken cancellationToken = default)
        {
            var counts = await _context.ExerciseLogs
                .AsNoTracking()
                .Where(el => el.UserId == userId)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalCount = g.Count(),
                    CompletedCount = g.Count(el => el.IsCompleted)
                })
                .SingleOrDefaultAsync(cancellationToken);

            var totalDurationTicks = (await _context.ExerciseLogs
                    .AsNoTracking()
                    .Where(el => el.UserId == userId && el.IsCompleted && el.Duration.HasValue)
                    .Select(el => el.Duration!.Value)
                    .ToListAsync(cancellationToken))
                .Sum(duration => duration.Ticks);

            return counts is null
                ? (0, 0, TimeSpan.Zero)
                : (counts.TotalCount, counts.CompletedCount, TimeSpan.FromTicks(totalDurationTicks));
        }

        public async Task AddAsync(ExerciseLog exerciseLog, CancellationToken cancellationToken = default)
        {
            await _context.ExerciseLogs.AddAsync(exerciseLog, cancellationToken);
        }

        public Task UpdateAsync(ExerciseLog exerciseLog, CancellationToken cancellationToken = default)
        {
            _context.ExerciseLogs.Update(exerciseLog);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(ExerciseLog exerciseLog, CancellationToken cancellationToken = default)
        {
            _context.ExerciseLogs.Remove(exerciseLog);
            return Task.CompletedTask;
        }
    }
}

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

        public async Task AddAsync(ExerciseLog exerciseLog, CancellationToken cancellationToken = default)
        {
            await _context.ExerciseLogs.AddAsync(exerciseLog, cancellationToken);
        }

        public Task UpdateAsync(ExerciseLog exerciseLog, CancellationToken cancellationToken = default)
        {
            _context.ExerciseLogs.Update(exerciseLog);
            return Task.CompletedTask;
        }
    }
}

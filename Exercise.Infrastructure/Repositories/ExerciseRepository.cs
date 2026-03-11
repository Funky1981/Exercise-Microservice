using Exercise.Application.Abstractions.Repositories;
using Exercise.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;

namespace Exercise.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core implementation of IExerciseRepository.
    /// Handles all database operations for the Exercise entity.
    /// </summary>
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly ExerciseDbContext _context;

        public ExerciseRepository(ExerciseDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ExerciseEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Exercises
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<(IReadOnlyList<ExerciseEntity> Items, int TotalCount)> GetPagedAsync(
            int skip, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.Exercises.AsNoTracking();
            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(e => e.Name)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            return (items, totalCount);
        }

        /// <inheritdoc />
        public async Task<ExerciseEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Exercises
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ExerciseEntity>> GetByBodyPartAsync(string bodyPart, CancellationToken cancellationToken = default)
        {
            return await _context.Exercises
                .AsNoTracking()
                .Where(e => EF.Functions.Like(e.BodyPart, bodyPart))
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddAsync(ExerciseEntity exercise, CancellationToken cancellationToken = default)
        {
            await _context.Exercises.AddAsync(exercise, cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateAsync(ExerciseEntity exercise, CancellationToken cancellationToken = default)
        {
            _context.Exercises.Update(exercise);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteAsync(ExerciseEntity exercise, CancellationToken cancellationToken = default)
        {
            _context.Exercises.Remove(exercise);
            return Task.CompletedTask;
        }
    }
}

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
            int skip,
            int pageSize,
            string? search = null,
            string? bodyPart = null,
            string? equipment = null,
            IReadOnlyCollection<string>? regionBodyParts = null,
            bool otherRegionOnly = false,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Exercises.AsNoTracking();

            if (regionBodyParts is { Count: > 0 })
            {
                query = query.Where(exercise => regionBodyParts.Contains(exercise.BodyPart));
            }
            else if (otherRegionOnly)
            {
                var excludedBodyParts = Exercise.Application.Features.Exercises.Support.ExerciseRegionCatalog.Regions
                    .Where(region => region != "other")
                    .SelectMany(Exercise.Application.Features.Exercises.Support.ExerciseRegionCatalog.GetBodyPartsForRegion)
                    .Distinct()
                    .ToList();
                query = query.Where(exercise => !excludedBodyParts.Contains(exercise.BodyPart));
            }

            if (!string.IsNullOrWhiteSpace(bodyPart))
            {
                query = query.Where(exercise => exercise.BodyPart == bodyPart);
            }

            if (!string.IsNullOrWhiteSpace(equipment))
            {
                query = query.Where(exercise => exercise.Equipment == equipment);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(exercise =>
                    exercise.Name.Contains(term)
                    || exercise.BodyPart.Contains(term)
                    || exercise.TargetMuscle.Contains(term)
                    || (exercise.Equipment != null && exercise.Equipment.Contains(term)));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderBy(e => e.Name)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            return (items, totalCount);
        }

        public async Task<IReadOnlyList<ExerciseEntity>> GetByIdsAsync(
            IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
        {
            if (ids.Count == 0)
            {
                return [];
            }

            var items = await _context.Exercises
                .Where(exercise => ids.Contains(exercise.Id))
                .ToListAsync(cancellationToken);

            return ids
                .Distinct()
                .Select(id => items.FirstOrDefault(exercise => exercise.Id == id))
                .Where(exercise => exercise is not null)
                .Cast<ExerciseEntity>()
                .ToList();
        }

        /// <inheritdoc />
        public async Task<ExerciseEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Exercises
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<ExerciseEntity?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Exercises
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Exercises.AnyAsync(e => e.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<ExerciseEntity>> GetByBodyPartAsync(string bodyPart, CancellationToken cancellationToken = default)
        {
            return await _context.Exercises
                .AsNoTracking()
                .Where(e => e.BodyPart == bodyPart)
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

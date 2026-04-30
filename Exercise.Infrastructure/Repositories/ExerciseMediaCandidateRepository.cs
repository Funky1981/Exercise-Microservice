using Exercise.Application.Abstractions.Repositories;
using Exercise.Domain.Entities;
using Exercise.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Exercise.Infrastructure.Repositories
{
    public sealed class ExerciseMediaCandidateRepository : IExerciseMediaCandidateRepository
    {
        private readonly ExerciseDbContext _context;

        public ExerciseMediaCandidateRepository(ExerciseDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<ExerciseMediaCandidate>> GetByExerciseIdAsync(Guid exerciseId, CancellationToken cancellationToken = default)
        {
            return await _context.ExerciseMediaCandidates
                .AsNoTracking()
                .Where(candidate => candidate.ExerciseId == exerciseId)
                .OrderByDescending(candidate => candidate.IsSelected)
                .ThenByDescending(candidate => candidate.MatchScore)
                .ThenByDescending(candidate => candidate.CreatedAt)
                .ToListAsync(cancellationToken);
        }

            public async Task<IReadOnlyList<ExerciseMediaCandidate>> GetPendingAsync(int limit = 100, CancellationToken cancellationToken = default)
            {
                return await _context.ExerciseMediaCandidates
                .AsNoTracking()
                .Include(candidate => candidate.Exercise)
                .Where(candidate => candidate.ReviewStatus == ExerciseMediaCandidateReviewStatuses.Pending)
                .OrderByDescending(candidate => candidate.MatchScore)
                .ThenByDescending(candidate => candidate.CreatedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);
            }

        public async Task<IReadOnlyList<ExerciseMediaCandidate>> GetByExerciseIdsForUpdateAsync(
            IReadOnlyCollection<Guid> exerciseIds,
            CancellationToken cancellationToken = default)
        {
            if (exerciseIds.Count == 0)
            {
                return [];
            }

            return await _context.ExerciseMediaCandidates
                .Where(candidate => exerciseIds.Contains(candidate.ExerciseId))
                .ToListAsync(cancellationToken);
        }

        public async Task<ExerciseMediaCandidate?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ExerciseMediaCandidates
                .FirstOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        }

        public async Task AddAsync(ExerciseMediaCandidate candidate, CancellationToken cancellationToken = default)
        {
            await _context.ExerciseMediaCandidates.AddAsync(candidate, cancellationToken);
        }
    }
}
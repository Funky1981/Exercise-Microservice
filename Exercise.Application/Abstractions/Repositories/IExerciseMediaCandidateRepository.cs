using Exercise.Domain.Entities;

namespace Exercise.Application.Abstractions.Repositories
{
    public interface IExerciseMediaCandidateRepository
    {
        Task<IReadOnlyList<ExerciseMediaCandidate>> GetByExerciseIdAsync(Guid exerciseId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ExerciseMediaCandidate>> GetPendingAsync(int limit = 100, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ExerciseMediaCandidate>> GetByExerciseIdsForUpdateAsync(
            IReadOnlyCollection<Guid> exerciseIds,
            CancellationToken cancellationToken = default);

        Task<ExerciseMediaCandidate?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

        Task AddAsync(ExerciseMediaCandidate candidate, CancellationToken cancellationToken = default);
    }
}
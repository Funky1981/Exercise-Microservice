using Exercise.Application.Abstractions.Repositories;
using Exercise.Infrastructure.Data;

namespace Exercise.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ExerciseDbContext _context;

        public UnitOfWork(ExerciseDbContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _context.SaveChangesAsync(cancellationToken);
    }
}

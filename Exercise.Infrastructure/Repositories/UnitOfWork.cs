using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using Exercise.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Exercise.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ExerciseDbContext _context;

        public UnitOfWork(ExerciseDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyException("The resource was modified by another request. Reload and try again.", ex);
            }
        }
    }
}

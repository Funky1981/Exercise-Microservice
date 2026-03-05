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

        public async Task<IReadOnlyList<Workout>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Workouts
                .AsNoTracking()
                .Where(w => w.UserId == userId)
                .ToListAsync(cancellationToken);
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

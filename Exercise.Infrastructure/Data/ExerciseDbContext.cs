using Microsoft.EntityFrameworkCore;
using Exercise.Domain.Entities;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Exercise.Infrastructure.Data
{
    /// <summary>
    /// Database context for the Exercise microservice.
    /// Manages database connections and entity mappings for all domain entities.
    /// </summary>
    public class ExerciseDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the ExerciseDbContext
        /// </summary>
        /// <param name="options">DbContext configuration options</param>
        public ExerciseDbContext(DbContextOptions<ExerciseDbContext> options) 
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the Exercises entity set
        /// Represents the Exercises table in the database
        /// </summary>
        public DbSet<ExerciseEntity> Exercises { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Users entity set
        /// Represents the Users table in the database
        /// </summary>
        public DbSet<User> Users { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Workouts entity set
        /// Represents the Workouts table in the database
        /// </summary>
        public DbSet<Workout> Workouts { get; set; } = null!;

        /// <summary>
        /// Gets or sets the WorkoutPlans entity set
        /// Represents the WorkoutPlans table in the database
        /// </summary>
        public DbSet<WorkoutPlan> WorkoutPlans { get; set; } = null!;

        /// <summary>
        /// Gets or sets the ExerciseLogs entity set
        /// Represents the ExerciseLogs table in the database
        /// </summary>
        public DbSet<ExerciseLog> ExerciseLogs { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Analytics entity set
        /// Represents the Analytics table in the database
        /// </summary>
        public DbSet<Analytics> Analytics { get; set; } = null!;

        /// <summary>
        /// Configures the entity models using Fluent API
        /// Applies all IEntityTypeConfiguration implementations from this assembly
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the model</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Automatically apply all entity configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExerciseDbContext).Assembly);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                if (entry.State == EntityState.Deleted && entry.Metadata.FindProperty("IsDeleted") is not null)
                {
                    // Convert hard delete to soft delete
                    entry.State = EntityState.Modified;
                    entry.Property("IsDeleted").CurrentValue = true;
                    entry.Property("UpdatedAt").CurrentValue = now;
                }

                if ((entry.State == EntityState.Added || entry.State == EntityState.Modified)
                    && entry.Metadata.FindProperty("UpdatedAt") is not null)
                {
                    entry.Property("UpdatedAt").CurrentValue = now;
                }

                StampConcurrencyToken(entry);
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        private static void StampConcurrencyToken(EntityEntry entry)
        {
            if (entry.Metadata.FindProperty("ConcurrencyToken") is null)
                return;

            entry.Property("ConcurrencyToken").CurrentValue = Guid.NewGuid().ToString("N");
        }
    }
}

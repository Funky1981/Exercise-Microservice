using Microsoft.EntityFrameworkCore;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;

namespace Exercise.Infrastructure.Data
{
    /// <summary>
    /// Database context for the Exercise microservice
    /// Manages database connections and entity mappings
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
    }
}

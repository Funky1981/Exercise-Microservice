using Exercise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;

namespace Exercise.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EF Core Fluent API configuration for the Workout entity.
    /// </summary>
    public class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
    {
        public void Configure(EntityTypeBuilder<Workout> builder)
        {
            builder.ToTable("Workouts");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.UserId)
                .IsRequired();

            builder.Property(w => w.Name)
                .HasMaxLength(200);

            builder.Property(w => w.Date)
                .IsRequired();

            builder.Property(w => w.Notes)
                .HasMaxLength(1000);

            builder.Property(w => w.IsCompleted)
                .IsRequired();

            // Index for common query pattern: fetch workouts by user
            builder.HasIndex(w => w.UserId);

            // Ignore the public read-only wrapper; map via the private backing field
            builder.Ignore(w => w.Exercises);

            // Many-to-many: Workout <-> Exercise via join table
            builder.HasMany<ExerciseEntity>("_exercises")
                .WithMany()
                .UsingEntity(j => j.ToTable("WorkoutExercises"));

            // Soft delete
            builder.Property<bool>("IsDeleted").HasDefaultValue(false);
            builder.Property<DateTime?>("UpdatedAt");
            builder.HasQueryFilter(w => !EF.Property<bool>(w, "IsDeleted"));
        }
    }
}

using Exercise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
            builder.HasIndex(w => new { w.UserId, w.Date });

            // Many-to-many: Workout <-> Exercise via join table.
            // UsePropertyAccessMode.Field tells EF Core to populate the private _exercises
            // backing field directly, making .Include(w => w.Exercises) type-safe.
            builder.HasMany(w => w.Exercises)
                .WithMany()
                .UsingEntity(j => j.ToTable("WorkoutExercises"));
            builder.Navigation(w => w.Exercises)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            // Soft delete
            builder.Property<bool>("IsDeleted").HasDefaultValue(false);
            builder.Property<DateTime?>("UpdatedAt");
            builder.Property<string>("ConcurrencyToken")
                .HasMaxLength(32)
                .HasDefaultValue(string.Empty)
                .IsConcurrencyToken();
            builder.HasQueryFilter(w => !EF.Property<bool>(w, "IsDeleted"));
        }
    }
}

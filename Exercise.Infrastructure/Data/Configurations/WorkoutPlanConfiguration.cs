using Exercise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exercise.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EF Core Fluent API configuration for the WorkoutPlan entity.
    /// </summary>
    public class WorkoutPlanConfiguration : IEntityTypeConfiguration<WorkoutPlan>
    {
        public void Configure(EntityTypeBuilder<WorkoutPlan> builder)
        {
            builder.ToTable("WorkoutPlans");

            builder.HasKey(wp => wp.Id);

            builder.Property(wp => wp.UserId)
                .IsRequired();

            builder.Property(wp => wp.Name)
                .HasMaxLength(200);

            builder.Property(wp => wp.StartDate)
                .IsRequired();

            builder.Property(wp => wp.Notes)
                .HasMaxLength(1000);

            builder.Property(wp => wp.IsActive)
                .IsRequired();

            builder.HasIndex(wp => new { wp.UserId, wp.StartDate });

            // Many-to-many: WorkoutPlan <-> Workout via join table.
            // UsePropertyAccessMode.Field tells EF Core to populate the private _workouts
            // backing field directly, making .Include(wp => wp.Workouts) type-safe.
            builder.HasMany(wp => wp.Workouts)
                .WithMany()
                .UsingEntity(j => j.ToTable("WorkoutPlanWorkouts"));
            builder.Navigation(wp => wp.Workouts)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            // Soft delete
            builder.Property<bool>("IsDeleted").HasDefaultValue(false);
            builder.Property<DateTime?>("UpdatedAt");
            builder.Property<string>("ConcurrencyToken")
                .HasMaxLength(32)
                .HasDefaultValue(string.Empty)
                .IsConcurrencyToken();
            builder.HasQueryFilter(wp => !EF.Property<bool>(wp, "IsDeleted"));
        }
    }
}

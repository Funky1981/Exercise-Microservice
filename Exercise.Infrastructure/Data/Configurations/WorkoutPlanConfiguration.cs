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

            builder.HasIndex(wp => wp.UserId);

            // Ignore the public read-only wrapper; map via the private backing field
            builder.Ignore(wp => wp.Workouts);

            // WorkoutPlan owns a collection of Workouts
            builder.HasMany<Workout>("_workouts")
                .WithMany()
                .UsingEntity(j => j.ToTable("WorkoutPlanWorkouts"));

            // Soft delete
            builder.Property<bool>("IsDeleted").HasDefaultValue(false);
            builder.Property<DateTime?>("UpdatedAt");
            builder.HasQueryFilter(wp => !EF.Property<bool>(wp, "IsDeleted"));
        }
    }
}

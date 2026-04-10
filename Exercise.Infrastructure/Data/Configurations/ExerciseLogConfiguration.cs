using Exercise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exercise.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EF Core Fluent API configuration for the ExerciseLog entity and its ExerciseLogEntry owned type.
    /// </summary>
    public class ExerciseLogConfiguration : IEntityTypeConfiguration<ExerciseLog>
    {
        public void Configure(EntityTypeBuilder<ExerciseLog> builder)
        {
            builder.ToTable("ExerciseLogs");

            builder.HasKey(el => el.Id);

            builder.Property(el => el.UserId)
                .IsRequired();

            builder.Property(el => el.WorkoutId);

            builder.Property(el => el.Name)
                .HasMaxLength(200);

            builder.Property(el => el.Date)
                .IsRequired();

            builder.Property(el => el.Notes)
                .HasMaxLength(1000);

            builder.Property(el => el.IsCompleted)
                .IsRequired();

            builder.HasIndex(el => new { el.UserId, el.Date });

            // The public IReadOnlyList property is a wrapper - tell EF to use the private backing field
            builder.Ignore(el => el.ExercisesCompleted);

            builder.OwnsMany<ExerciseLogEntry>("_exercisesCompleted", entry =>
            {
                entry.ToTable("ExerciseLogEntries");

                entry.WithOwner().HasForeignKey("ExerciseLogId");

                entry.Property(e => e.ExerciseId).IsRequired();
                entry.Property(e => e.Sets).IsRequired();
                entry.Property(e => e.Reps).IsRequired();
                entry.Property(e => e.Duration);
                entry.Property(e => e.RestTime);
                entry.Property(e => e.CompletedAt);
            });

            // Soft delete
            builder.Property<bool>("IsDeleted").HasDefaultValue(false);
            builder.Property<DateTime?>("UpdatedAt");
            builder.Property<string>("ConcurrencyToken")
                .HasMaxLength(32)
                .HasDefaultValue(string.Empty)
                .IsConcurrencyToken();
            builder.HasQueryFilter(el => !EF.Property<bool>(el, "IsDeleted"));
        }
    }
}

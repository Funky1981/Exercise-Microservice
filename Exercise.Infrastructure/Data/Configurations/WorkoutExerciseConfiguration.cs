using Exercise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exercise.Infrastructure.Data.Configurations
{
    public class WorkoutExerciseConfiguration : IEntityTypeConfiguration<WorkoutExercise>
    {
        public void Configure(EntityTypeBuilder<WorkoutExercise> builder)
        {
            builder.ToTable("WorkoutExercises");

            builder.HasKey(we => we.Id);

            builder.Property(we => we.WorkoutId).IsRequired();
            builder.Property(we => we.ExerciseId).IsRequired();
            builder.Property(we => we.Sets).IsRequired();
            builder.Property(we => we.Reps).IsRequired();
            builder.Property(we => we.RestSeconds).IsRequired();
            builder.Property(we => we.Order).IsRequired();

            builder.HasOne(we => we.Exercise)
                .WithMany()
                .HasForeignKey(we => we.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(we => new { we.WorkoutId, we.ExerciseId }).IsUnique();
            builder.HasIndex(we => new { we.WorkoutId, we.Order });
        }
    }
}

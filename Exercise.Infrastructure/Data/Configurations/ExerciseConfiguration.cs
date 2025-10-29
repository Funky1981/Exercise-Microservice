using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;


namespace Exercise.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Configuration for the Exercise entity
    /// Defines how the Exercise entity maps to the database schema
    /// </summary>
    public class ExerciseConfiguration : IEntityTypeConfiguration<ExerciseEntity>
    {
        /// <summary>
        /// Configures the Exercise entity properties and relationships
        /// </summary>
        /// <param name="builder">The builder used to configure the entity</param>
        public void Configure(EntityTypeBuilder<ExerciseEntity> builder)
        {
            builder.ToTable("Exercises");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.BodyPart)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.TargetMuscle)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Equipment)
                .HasMaxLength(100);

            builder.Property(e => e.GifUrl)
                .HasMaxLength(500);

            builder.Property(e => e.Description)
                .HasMaxLength(1000);

            builder.Property(e => e.Difficulty)
                .HasMaxLength(50);
            
            builder.HasIndex(e => e.BodyPart);
        }
    }
}
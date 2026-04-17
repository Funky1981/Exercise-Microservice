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

            builder.Property(e => e.ExternalId)
                .HasMaxLength(100);

            builder.Property(e => e.SourceProvider)
                .HasMaxLength(100);

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

            builder.Property(e => e.MediaUrl)
                .HasMaxLength(500);

            builder.Property(e => e.MediaKind)
                .HasMaxLength(50);

            builder.Property(e => e.SecondaryMusclesJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.InstructionsJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.SourcePayloadJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(e => e.Description)
                .HasMaxLength(1000);

            builder.Property(e => e.Difficulty)
                .HasMaxLength(50);

            builder.Property(e => e.Category)
                .HasMaxLength(100);
            
            builder.HasIndex(e => e.BodyPart);
            builder.HasIndex(e => e.ExternalId);

            // Soft delete
            builder.Property<bool>("IsDeleted").HasDefaultValue(false);
            builder.Property<DateTime?>("UpdatedAt");
            builder.Property<string>("ConcurrencyToken")
                .HasMaxLength(32)
                .HasDefaultValue(string.Empty)
                .IsConcurrencyToken();
            builder.HasQueryFilter(e => !EF.Property<bool>(e, "IsDeleted"));
        }
    }
}

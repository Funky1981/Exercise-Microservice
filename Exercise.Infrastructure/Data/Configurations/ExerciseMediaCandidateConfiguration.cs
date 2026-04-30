using Exercise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exercise.Infrastructure.Data.Configurations
{
    public sealed class ExerciseMediaCandidateConfiguration : IEntityTypeConfiguration<ExerciseMediaCandidate>
    {
        public void Configure(EntityTypeBuilder<ExerciseMediaCandidate> builder)
        {
            builder.ToTable("ExerciseMediaCandidates");

            builder.HasKey(candidate => candidate.Id);

            builder.Property(candidate => candidate.MediaUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(candidate => candidate.MediaKind)
                .HasMaxLength(50);

            builder.Property(candidate => candidate.ThumbnailUrl)
                .HasMaxLength(500);

            builder.Property(candidate => candidate.SourcePageUrl)
                .HasMaxLength(500);

            builder.Property(candidate => candidate.SourceProvider)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(candidate => candidate.SourcePayloadJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(candidate => candidate.SourceTitle)
                .HasMaxLength(300);

            builder.Property(candidate => candidate.MatchScore)
                .HasPrecision(5, 4);

            builder.Property(candidate => candidate.ReviewStatus)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(candidate => candidate.ReviewNotes)
                .HasMaxLength(500);

            builder.Property(candidate => candidate.CreatedAt)
                .IsRequired();

            builder.HasIndex(candidate => candidate.ExerciseId);
            builder.HasIndex(candidate => new { candidate.ExerciseId, candidate.SourceProvider, candidate.MediaUrl })
                .IsUnique();

            builder.HasOne(candidate => candidate.Exercise)
                .WithMany(exercise => exercise.MediaCandidates)
                .HasForeignKey(candidate => candidate.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(candidate => !EF.Property<bool>(candidate.Exercise!, "IsDeleted"));
        }
    }
}
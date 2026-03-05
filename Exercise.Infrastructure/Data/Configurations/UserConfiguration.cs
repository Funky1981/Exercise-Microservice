using Exercise.Domain.Entities;
using Exercise.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exercise.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EF Core Fluent API configuration for the User entity.
    /// </summary>
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.UserName)
                .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                .HasMaxLength(500);

            builder.Property(u => u.Provider)
                .HasMaxLength(100);

            builder.Property(u => u.ProviderId)
                .HasMaxLength(200);

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            // Map Height value object as owned entity (stored as columns)
            builder.OwnsOne(u => u.Height, h =>
            {
                h.Property(x => x.Centimeters)
                    .HasColumnName("HeightCm")
                    .HasPrecision(6, 2);
            });

            // Map Weight value object as owned entity (stored as columns)
            builder.OwnsOne(u => u.Weight, w =>
            {
                w.Property(x => x.Kilograms)
                    .HasColumnName("WeightKg")
                    .HasPrecision(6, 2);
            });

            // Soft delete
            builder.Property<bool>("IsDeleted").HasDefaultValue(false);
            builder.Property<DateTime?>("UpdatedAt");
            builder.HasQueryFilter(u => !EF.Property<bool>(u, "IsDeleted"));
        }
    }
}

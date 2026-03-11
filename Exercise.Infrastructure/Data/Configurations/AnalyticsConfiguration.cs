using System.Text.Json;
using Exercise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exercise.Infrastructure.Data.Configurations
{
    /// <summary>
    /// EF Core Fluent API configuration for the Analytics entity.
    /// The <c>Data</c> property is persisted as a JSON string because
    /// <c>Dictionary&lt;string, object&gt;</c> has no direct column mapping.
    /// </summary>
    public class AnalyticsConfiguration : IEntityTypeConfiguration<Analytics>
    {
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public void Configure(EntityTypeBuilder<Analytics> builder)
        {
            builder.ToTable("Analytics");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.UserId)
                .IsRequired();

            builder.Property(a => a.AnalyticsType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.PeriodStart)
                .IsRequired();

            builder.Property(a => a.PeriodEnd)
                .IsRequired();

            builder.Property(a => a.GeneratedAt)
                .IsRequired();

            // Store the flexible Data dictionary as a JSON column
            builder.Property(a => a.Data)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, _jsonOptions),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, _jsonOptions)
                         ?? new Dictionary<string, object>());

            // Index for common query pattern: fetch analytics by user
            builder.HasIndex(a => a.UserId);

            // Soft delete
            builder.Property<bool>("IsDeleted").HasDefaultValue(false);
            builder.Property<DateTime?>("UpdatedAt");
            builder.HasQueryFilter(a => !EF.Property<bool>(a, "IsDeleted"));
        }
    }
}

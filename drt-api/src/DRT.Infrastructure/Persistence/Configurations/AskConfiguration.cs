using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DRT.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the Ask entity.
/// Maps to the CAW_Ask table.
/// Concurrency token: RowVersion (SQL rowversion).
/// </summary>
public sealed class AskConfiguration : IEntityTypeConfiguration<Ask>
{
    public void Configure(EntityTypeBuilder<Ask> builder)
    {
        builder.ToTable("CAW_Ask");

        builder.HasKey(x => x.AskId);

        builder.Property(x => x.AskId)
            .HasColumnName("AskId")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Version)
            .HasColumnName("Version")
            .IsRequired();

        builder.Property(x => x.StatusId)
            .HasColumnName("StatusId")
            .IsRequired();

        builder.Property(x => x.CreatedByActorId)
            .HasColumnName("CreatedByActorId")
            .IsRequired();

        builder.Property(x => x.CreatedUtc)
            .HasColumnName("CreatedUtc")
            .IsRequired();

        builder.Property(x => x.CancelledAt)
            .HasColumnName("CancelledAt")
            .IsRequired(false);

        builder.Property(x => x.CancelledBy)
            .HasColumnName("CancelledBy")
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(x => x.ModifiedBy)
            .HasColumnName("ModifiedBy")
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(x => x.ModifiedOn)
            .HasColumnName("ModifiedOn")
            .IsRequired(false);

        // Optimistic concurrency token (SQL rowversion)
        builder.Property(x => x.RowVersion)
            .HasColumnName("RowVersion")
            .IsRowVersion();
    }
}

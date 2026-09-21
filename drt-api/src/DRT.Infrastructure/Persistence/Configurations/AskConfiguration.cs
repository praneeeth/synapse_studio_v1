using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DRT.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the Ask aggregate root.
/// Maps to the CAW_Ask table.
/// </summary>
public sealed class AskConfiguration : IEntityTypeConfiguration<Ask>
{
    public void Configure(EntityTypeBuilder<Ask> builder)
    {
        builder.ToTable("CAW_Ask");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("AskId")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.StatusId)
            .HasColumnName("StatusId")
            .IsRequired();

        builder.Property(a => a.Version)
            .HasColumnName("Version")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("CreatedOn")
            .IsRequired();

        builder.Property(a => a.CreatedBy)
            .HasColumnName("CreatedBy")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("ModifiedOn")
            .IsRequired(false);

        builder.Property(a => a.UpdatedBy)
            .HasColumnName("ModifiedBy")
            .IsRequired(false);

        // Cancellation fields (US-ASK-015)
        builder.Property(a => a.CancelledAt)
            .HasColumnName("CancelledAt")
            .IsRequired(false);

        builder.Property(a => a.CancelledBy)
            .HasColumnName("CancelledBy")
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(a => a.ModifiedBy)
            .HasColumnName("CancelModifiedBy")
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(a => a.ModifiedOn)
            .HasColumnName("CancelModifiedOn")
            .IsRequired(false);

        // Concurrency token — maps to the RowVersion / timestamp column.
        // TODO: US-ASK-015 - Confirm the exact column name and type (rowversion vs. int version)
        // used for optimistic concurrency in CAW_Ask. Using RowVersion byte[] as default.
        builder.Property(a => a.RowVersion)
            .HasColumnName("RowVersion")
            .IsRowVersion()
            .IsConcurrencyToken();

        // Ignore the Status enum navigation — StatusId is the persisted field.
        builder.Ignore(a => a.Status);
    }
}

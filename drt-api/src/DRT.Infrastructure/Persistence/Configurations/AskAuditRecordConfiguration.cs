using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DRT.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for AskAuditRecord.
/// Maps to the audit table for ASK operations.
/// TODO: US-ASK-015 - Confirm the exact audit table name in the DRT database schema.
/// </summary>
public sealed class AskAuditRecordConfiguration : IEntityTypeConfiguration<AskAuditRecord>
{
    public void Configure(EntityTypeBuilder<AskAuditRecord> builder)
    {
        // TODO: US-ASK-015 - Confirm the exact audit table name (e.g. CAW_AskAudit, AuditLog, etc.).
        builder.ToTable("CAW_AskAudit");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("AuditId")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.AskId)
            .HasColumnName("AskId")
            .IsRequired();

        builder.Property(a => a.Action)
            .HasColumnName("Action")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnName("Description")
            .HasMaxLength(4000)
            .IsRequired(false);

        builder.Property(a => a.ActorId)
            .HasColumnName("ActorId")
            .IsRequired();

        builder.Property(a => a.CreatedByIdentity)
            .HasColumnName("CreatedByIdentity")
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(a => a.OccurredAt)
            .HasColumnName("CreatedOn")
            .IsRequired();

        builder.Property(a => a.CorrelationId)
            .HasColumnName("CorrelationId")
            .HasMaxLength(128)
            .IsRequired(false);
    }
}

using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DRT.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the AuditRecord entity.
/// </summary>
public sealed class AuditRecordConfiguration : IEntityTypeConfiguration<AuditRecord>
{
    public void Configure(EntityTypeBuilder<AuditRecord> builder)
    {
        builder.ToTable("CAW_AuditRecord");

        builder.HasKey(x => x.AuditId);

        builder.Property(x => x.AuditId)
            .HasColumnName("AuditId")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.AskId)
            .HasColumnName("AskId")
            .IsRequired();

        builder.Property(x => x.ActorId)
            .HasColumnName("ActorId")
            .IsRequired();

        builder.Property(x => x.Operation)
            .HasColumnName("Action")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("Description")
            .IsRequired(false);

        builder.Property(x => x.CreatedBy)
            .HasColumnName("CreatedBy")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.OccurredUtc)
            .HasColumnName("CreatedOn")
            .IsRequired();
    }
}

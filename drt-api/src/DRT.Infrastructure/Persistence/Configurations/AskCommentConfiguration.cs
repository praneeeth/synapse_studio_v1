using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DRT.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for AskComment.
/// Maps to the CAW_Comment table.
/// </summary>
public sealed class AskCommentConfiguration : IEntityTypeConfiguration<AskComment>
{
    public void Configure(EntityTypeBuilder<AskComment> builder)
    {
        builder.ToTable("CAW_Comment");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("CommentId")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.AskId)
            .HasColumnName("AskId")
            .IsRequired();

        builder.Property(c => c.RequestId)
            .HasColumnName("RequestId")
            .IsRequired();

        // TODO: US-ASK-015 - Confirm the moduleTypeId column name in CAW_Comment.
        builder.Property(c => c.ModuleTypeId)
            .HasColumnName("ModuleTypeId")
            .IsRequired();

        builder.Property(c => c.Text)
            .HasColumnName("Comment")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .HasColumnName("IsActive")
            .IsRequired();

        builder.Property(c => c.IsDraft)
            .HasColumnName("IsDraft")
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasColumnName("CreatedBy")
            .IsRequired();

        builder.Property(c => c.CreatedByIdentity)
            .HasColumnName("CreatedByIdentity")
            .HasMaxLength(256)
            .IsRequired(false);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("CreatedOn")
            .IsRequired();
    }
}

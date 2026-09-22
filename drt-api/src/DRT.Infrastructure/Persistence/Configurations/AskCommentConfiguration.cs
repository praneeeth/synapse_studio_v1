using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DRT.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core Fluent API configuration for the AskComment entity.
/// Maps to the CAW_Comment table.
/// </summary>
public sealed class AskCommentConfiguration : IEntityTypeConfiguration<AskComment>
{
    public void Configure(EntityTypeBuilder<AskComment> builder)
    {
        builder.ToTable("CAW_Comment");

        builder.HasKey(x => x.AskCommentId);

        builder.Property(x => x.AskCommentId)
            .HasColumnName("CommentId")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.AskId)
            .HasColumnName("AskId")
            .IsRequired();

        builder.Property(x => x.RequestId)
            .HasColumnName("RequestId")
            .IsRequired();

        builder.Property(x => x.ModuleTypeId)
            .HasColumnName("ModuleTypeId")
            .IsRequired();

        builder.Property(x => x.CommentText)
            .HasColumnName("Comment")
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("IsActive")
            .IsRequired();

        builder.Property(x => x.IsDraft)
            .HasColumnName("IsDraft")
            .IsRequired();

        builder.Property(x => x.CreatedByActorId)
            .HasColumnName("CreatedByActorId")
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasColumnName("CreatedBy")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.CreatedUtc)
            .HasColumnName("CreatedOn")
            .IsRequired();
    }
}

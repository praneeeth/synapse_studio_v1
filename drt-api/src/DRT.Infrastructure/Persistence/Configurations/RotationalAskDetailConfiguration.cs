using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DRT.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="RotationalAskDetail"/>.
/// </summary>
public sealed class RotationalAskDetailConfiguration : IEntityTypeConfiguration<RotationalAskDetail>
{
    public void Configure(EntityTypeBuilder<RotationalAskDetail> builder)
    {
        builder.ToTable("RotationalAskDetails");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .UseIdentityColumn();

        builder.Property(x => x.AskId)
            .IsRequired();

        // nameOfTheAsk: Required for Rotational ASK, max 100 characters (BRD RT_August_Release).
        builder.Property(x => x.NameOfTheAsk)
            .HasMaxLength(100);

        builder.Property(x => x.Year);

        builder.HasIndex(x => x.AskId)
            .HasDatabaseName("IX_RotationalAskDetails_AskId");
    }
}

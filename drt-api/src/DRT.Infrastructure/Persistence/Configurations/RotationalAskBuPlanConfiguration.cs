using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DRT.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for <see cref="RotationalAskBuPlan"/>.
/// </summary>
public sealed class RotationalAskBuPlanConfiguration : IEntityTypeConfiguration<RotationalAskBuPlan>
{
    public void Configure(EntityTypeBuilder<RotationalAskBuPlan> builder)
    {
        builder.ToTable("RotationalAskBuPlans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .UseIdentityColumn();

        builder.Property(x => x.AskId)
            .IsRequired();

        // TODO: ASK-POST-CORE-ASKS - BuId max length must be confirmed from reference data schema.
        builder.Property(x => x.BuId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Plan)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasIndex(x => x.AskId)
            .HasDatabaseName("IX_RotationalAskBuPlans_AskId");
    }
}

using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DRT.Infrastructure.Persistence.Configurations;

public sealed class CoreAskDetailConfiguration : IEntityTypeConfiguration<CoreAskDetail>
{
    public void Configure(EntityTypeBuilder<CoreAskDetail> builder)
    {
        builder.HasKey(x => x.CoreAskDetailId);
        builder.Property(x => x.CoreAskDetailId).ValueGeneratedOnAdd();
        builder.Property(x => x.AskId).IsRequired();
        builder.Property(x => x.CoreAskName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Pml).HasMaxLength(99);
        builder.Property(x => x.TransitionalCoach).HasMaxLength(99);
        builder.Property(x => x.RoleSummary).IsRequired();
        builder.Property(x => x.RoleResponsibility).IsRequired();
        builder.Property(x => x.RoleQualification).IsRequired();
        builder.Property(x => x.HeadCountAmount).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(x => x.FteAmount).IsRequired().HasColumnType("decimal(18,4)");
    }
}

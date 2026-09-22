using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DRT.Infrastructure.Persistence.Configurations;

public sealed class AskConfiguration : IEntityTypeConfiguration<Ask>
{
    public void Configure(EntityTypeBuilder<Ask> builder)
    {
        builder.HasKey(x => x.AskId);
        builder.Property(x => x.AskId).ValueGeneratedOnAdd();
        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.CreatedByActorId).IsRequired();
        builder.Property(x => x.CreatedUtc).IsRequired();
    }
}

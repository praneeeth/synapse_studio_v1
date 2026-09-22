using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DRT.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(x => x.MessageId);
        builder.Property(x => x.MessageType).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.OccurredUtc).IsRequired();
        builder.Property(x => x.ProcessingStatus).IsRequired().HasMaxLength(50);
        builder.Property(x => x.AttemptCount).IsRequired();
    }
}

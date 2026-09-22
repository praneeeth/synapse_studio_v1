using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DRT.Infrastructure.Persistence.Configurations;

public sealed class WorkflowTaskConfiguration : IEntityTypeConfiguration<WorkflowTask>
{
    public void Configure(EntityTypeBuilder<WorkflowTask> builder)
    {
        builder.HasKey(x => x.TaskId);
        builder.Property(x => x.TaskId).ValueGeneratedOnAdd();
        builder.Property(x => x.AskId).IsRequired();
        builder.Property(x => x.StatusId).IsRequired();
        builder.Property(x => x.AssigneeId).IsRequired();
        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.CreatedUtc).IsRequired();
    }
}

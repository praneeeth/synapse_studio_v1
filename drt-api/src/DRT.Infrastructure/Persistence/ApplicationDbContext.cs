using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DRT.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Ask> Asks => Set<Ask>();
    public DbSet<AskVersion> AskVersions => Set<AskVersion>();
    public DbSet<CoreAskDetail> CoreAskDetails => Set<CoreAskDetail>();
    public DbSet<AskComment> AskComments => Set<AskComment>();
    public DbSet<AskAttachmentLink> AskAttachmentLinks => Set<AskAttachmentLink>();
    public DbSet<WorkflowTask> WorkflowTasks => Set<WorkflowTask>();
    public DbSet<AskAuditRecord> AskAuditRecords => Set<AskAuditRecord>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    // Rotational ASK entities (ASK-POST-CORE-ASKS)
    public DbSet<RotationalAskDetail> RotationalAskDetails => Set<RotationalAskDetail>();
    public DbSet<RotationalAskBuPlan> RotationalAskBuPlans => Set<RotationalAskBuPlan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

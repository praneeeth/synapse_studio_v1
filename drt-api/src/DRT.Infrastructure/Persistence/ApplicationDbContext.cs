using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DRT.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for the DRT application.
/// All entity configurations are applied via IEntityTypeConfiguration classes
/// using ApplyConfigurationsFromAssembly (Fluent API, no Data Annotations on domain entities).
/// </summary>
public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // Core ASK aggregate
    public DbSet<Ask> Asks => Set<Ask>();
    public DbSet<AskVersion> AskVersions => Set<AskVersion>();
    public DbSet<CoreAskDetail> CoreAskDetails => Set<CoreAskDetail>();
    public DbSet<AskComment> AskComments => Set<AskComment>();
    public DbSet<AskAttachmentLink> AskAttachmentLinks => Set<AskAttachmentLink>();
    public DbSet<AskAuditRecord> AskAuditRecords => Set<AskAuditRecord>();
    public DbSet<WorkflowTask> WorkflowTasks => Set<WorkflowTask>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    // Rotational ASK
    public DbSet<RotationalAskDetail> RotationalAskDetails => Set<RotationalAskDetail>();
    public DbSet<RotationalAskBuPlan> RotationalAskBuPlans => Set<RotationalAskBuPlan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all IEntityTypeConfiguration<T> implementations from this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

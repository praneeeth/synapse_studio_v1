using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DRT.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for the DRT application.
/// </summary>
public sealed class ApplicationDbContext : DbContext, DRT.Application.Abstractions.IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Ask> Asks => Set<Ask>();
    public DbSet<AskVersion> AskVersions => Set<AskVersion>();
    public DbSet<CoreAskDetail> CoreAskDetails => Set<CoreAskDetail>();
    public DbSet<AskComment> AskComments => Set<AskComment>();
    public DbSet<AskAttachmentLink> AskAttachmentLinks => Set<AskAttachmentLink>();
    public DbSet<WorkflowTask> WorkflowTasks => Set<WorkflowTask>();
    public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

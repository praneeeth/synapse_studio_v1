using DRT.Application.Abstractions;
using DRT.Domain.Entities;

namespace DRT.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for the unified Create ASK use case (ASK-POST-CORE-ASKS).
/// </summary>
public sealed class CreateAskRepository : ICreateAskRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CreateAskRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAskAsync(Ask ask, CancellationToken cancellationToken)
        => await _dbContext.Asks.AddAsync(ask, cancellationToken);

    public async Task AddAskVersionAsync(AskVersion askVersion, CancellationToken cancellationToken)
        => await _dbContext.AskVersions.AddAsync(askVersion, cancellationToken);

    public async Task AddCoreAskDetailAsync(CoreAskDetail detail, CancellationToken cancellationToken)
        => await _dbContext.CoreAskDetails.AddAsync(detail, cancellationToken);

    public async Task AddRotationalAskDetailAsync(RotationalAskDetail detail, CancellationToken cancellationToken)
        => await _dbContext.RotationalAskDetails.AddAsync(detail, cancellationToken);

    public async Task AddRotationalAskBuPlanAsync(RotationalAskBuPlan plan, CancellationToken cancellationToken)
        => await _dbContext.RotationalAskBuPlans.AddAsync(plan, cancellationToken);

    public async Task AddAskCommentAsync(AskComment comment, CancellationToken cancellationToken)
        => await _dbContext.AskComments.AddAsync(comment, cancellationToken);

    public async Task AddWorkflowTaskAsync(WorkflowTask task, CancellationToken cancellationToken)
        => await _dbContext.WorkflowTasks.AddAsync(task, cancellationToken);

    public async Task AddAuditRecordAsync(AskAuditRecord audit, CancellationToken cancellationToken)
        => await _dbContext.AskAuditRecords.AddAsync(audit, cancellationToken);

    public async Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
        => await _dbContext.OutboxMessages.AddAsync(message, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _dbContext.SaveChangesAsync(cancellationToken);
}

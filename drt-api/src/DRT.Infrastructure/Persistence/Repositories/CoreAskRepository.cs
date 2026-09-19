using DRT.Application.Abstractions;
using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DRT.Infrastructure.Persistence.Repositories;

public sealed class CoreAskRepository : ICoreAskRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CoreAskRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAskAsync(Ask ask, CancellationToken cancellationToken)
        => await _dbContext.Asks.AddAsync(ask, cancellationToken);

    public async Task AddAskVersionAsync(AskVersion askVersion, CancellationToken cancellationToken)
        => await _dbContext.AskVersions.AddAsync(askVersion, cancellationToken);

    public async Task AddCoreAskDetailAsync(CoreAskDetail detail, CancellationToken cancellationToken)
        => await _dbContext.CoreAskDetails.AddAsync(detail, cancellationToken);

    public async Task AddAskCommentAsync(AskComment comment, CancellationToken cancellationToken)
        => await _dbContext.AskComments.AddAsync(comment, cancellationToken);

    public async Task AddAskAttachmentLinkAsync(AskAttachmentLink link, CancellationToken cancellationToken)
        => await _dbContext.AskAttachmentLinks.AddAsync(link, cancellationToken);

    public async Task AddWorkflowTaskAsync(WorkflowTask task, CancellationToken cancellationToken)
        => await _dbContext.WorkflowTasks.AddAsync(task, cancellationToken);

    public async Task AddAuditRecordAsync(AskAuditRecord audit, CancellationToken cancellationToken)
        => await _dbContext.AskAuditRecords.AddAsync(audit, cancellationToken);

    public async Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
        => await _dbContext.OutboxMessages.AddAsync(message, cancellationToken);

    /// <summary>
    /// Loads a Core ASK by its primary key. Returns null if not found.
    /// Used by the Cancel Core ASK use case (US-ASK-015).
    /// </summary>
    public async Task<Ask?> GetByIdAsync(int askId, CancellationToken cancellationToken)
        => await _dbContext.Asks
            .FirstOrDefaultAsync(a => a.Id == askId, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _dbContext.SaveChangesAsync(cancellationToken);
}

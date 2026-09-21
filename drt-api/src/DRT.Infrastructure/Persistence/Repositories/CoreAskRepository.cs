using DRT.Application.Abstractions;
using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DRT.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of ICoreAskRepository.
/// Handles persistence for Core ASK aggregate operations including cancellation (US-ASK-015).
/// </summary>
public sealed class CoreAskRepository : ICoreAskRepository
{
    private readonly ApplicationDbContext _db;

    public CoreAskRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAskAsync(Ask ask, CancellationToken cancellationToken)
    {
        await _db.Asks.AddAsync(ask, cancellationToken);
    }

    public async Task AddAskVersionAsync(AskVersion askVersion, CancellationToken cancellationToken)
    {
        await _db.AskVersions.AddAsync(askVersion, cancellationToken);
    }

    public async Task AddCoreAskDetailAsync(CoreAskDetail detail, CancellationToken cancellationToken)
    {
        await _db.CoreAskDetails.AddAsync(detail, cancellationToken);
    }

    public async Task AddAskCommentAsync(AskComment comment, CancellationToken cancellationToken)
    {
        await _db.AskComments.AddAsync(comment, cancellationToken);
    }

    public async Task AddAskAttachmentLinkAsync(AskAttachmentLink link, CancellationToken cancellationToken)
    {
        await _db.AskAttachmentLinks.AddAsync(link, cancellationToken);
    }

    public async Task AddWorkflowTaskAsync(WorkflowTask task, CancellationToken cancellationToken)
    {
        await _db.WorkflowTasks.AddAsync(task, cancellationToken);
    }

    public async Task AddAuditRecordAsync(AskAuditRecord audit, CancellationToken cancellationToken)
    {
        await _db.AskAuditRecords.AddAsync(audit, cancellationToken);
    }

    public async Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        await _db.OutboxMessages.AddAsync(message, cancellationToken);
    }

    /// <summary>
    /// Loads a Core ASK by its primary key. Returns null if not found.
    /// Used by the Cancel Core ASK use case (US-ASK-015).
    /// </summary>
    public async Task<Ask?> GetByIdAsync(int askId, CancellationToken cancellationToken)
    {
        return await _db.Asks
            .FirstOrDefaultAsync(a => a.Id == askId, cancellationToken);
    }

    /// <summary>
    /// Persists all tracked changes in a single EF Core transaction.
    /// Translates DbUpdateConcurrencyException into AskConcurrencyException
    /// to keep the Application layer free of EF Core dependencies.
    /// </summary>
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new AskConcurrencyException(
                "The ASK has been modified by another user.", ex);
        }
    }
}

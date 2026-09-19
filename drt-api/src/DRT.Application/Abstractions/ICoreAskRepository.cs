using DRT.Domain.Entities;

namespace DRT.Application.Abstractions;

/// <summary>
/// Persistence port for Core ASK aggregate operations.
/// </summary>
public interface ICoreAskRepository
{
    Task AddAskAsync(Ask ask, CancellationToken cancellationToken);
    Task AddAskVersionAsync(AskVersion askVersion, CancellationToken cancellationToken);
    Task AddCoreAskDetailAsync(CoreAskDetail detail, CancellationToken cancellationToken);
    Task AddAskCommentAsync(AskComment comment, CancellationToken cancellationToken);
    Task AddAskAttachmentLinkAsync(AskAttachmentLink link, CancellationToken cancellationToken);
    Task AddWorkflowTaskAsync(WorkflowTask task, CancellationToken cancellationToken);
    Task AddAuditRecordAsync(AskAuditRecord audit, CancellationToken cancellationToken);
    Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

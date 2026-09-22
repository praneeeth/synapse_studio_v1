using DRT.Domain.Entities;

namespace DRT.Application.Abstractions;

/// <summary>
/// Port for persisting and reading Core ASK aggregate entities.
/// </summary>
public interface ICoreAskRepository
{
    Task<Ask?> GetByIdAsync(int askId, CancellationToken cancellationToken = default);
    Task AddAskAsync(Ask ask, CancellationToken cancellationToken = default);
    Task AddAskVersionAsync(AskVersion askVersion, CancellationToken cancellationToken = default);
    Task AddCoreAskDetailAsync(CoreAskDetail detail, CancellationToken cancellationToken = default);
    Task AddAskCommentAsync(AskComment comment, CancellationToken cancellationToken = default);
    Task AddAskAttachmentLinkAsync(AskAttachmentLink link, CancellationToken cancellationToken = default);
    Task AddWorkflowTaskAsync(WorkflowTask task, CancellationToken cancellationToken = default);
}

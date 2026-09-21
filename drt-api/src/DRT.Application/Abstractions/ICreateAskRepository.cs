using DRT.Domain.Entities;

namespace DRT.Application.Abstractions;

/// <summary>
/// Persistence port for the unified Create ASK use case (ASK-POST-CORE-ASKS).
/// Covers both Core and Rotational ASK creation.
/// </summary>
public interface ICreateAskRepository
{
    Task AddAskAsync(Ask ask, CancellationToken cancellationToken);
    Task AddAskVersionAsync(AskVersion askVersion, CancellationToken cancellationToken);
    Task AddCoreAskDetailAsync(CoreAskDetail detail, CancellationToken cancellationToken);
    Task AddRotationalAskDetailAsync(RotationalAskDetail detail, CancellationToken cancellationToken);
    Task AddRotationalAskBuPlanAsync(RotationalAskBuPlan plan, CancellationToken cancellationToken);
    Task AddAskCommentAsync(AskComment comment, CancellationToken cancellationToken);
    Task AddWorkflowTaskAsync(WorkflowTask task, CancellationToken cancellationToken);
    Task AddAuditRecordAsync(AskAuditRecord audit, CancellationToken cancellationToken);
    Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

using DRT.Domain.Entities;

namespace DRT.Application.Abstractions;

/// <summary>
/// Port for persisting outbox messages.
/// </summary>
public interface IOutboxRepository
{
    Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}

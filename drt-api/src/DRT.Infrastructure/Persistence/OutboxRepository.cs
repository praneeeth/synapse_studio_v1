using DRT.Application.Abstractions;
using DRT.Domain.Entities;

namespace DRT.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for outbox messages.
/// </summary>
public sealed class OutboxRepository : IOutboxRepository
{
    private readonly ApplicationDbContext _dbContext;

    public OutboxRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddOutboxMessageAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        => await _dbContext.OutboxMessages.AddAsync(message, cancellationToken);
}

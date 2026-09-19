namespace DRT.Domain.Entities;

/// <summary>
/// Transactional outbox message for post-commit notification processing.
/// </summary>
public sealed class OutboxMessage
{
    public int Id { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string AggregateType { get; private set; } = string.Empty;
    public int AggregateId { get; private set; }
    public string Payload { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Pending";
    public int Attempts { get; private set; }
    public DateTimeOffset? NextAttemptAt { get; private set; }
    public string? LeaseOwner { get; private set; }
    public DateTimeOffset? LeaseUntil { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? LastError { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string? CorrelationId { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(
        string eventType,
        string aggregateType,
        int aggregateId,
        string payload,
        DateTimeOffset now,
        string? correlationId)
    {
        return new OutboxMessage
        {
            EventType = eventType,
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            Payload = payload,
            Status = "Pending",
            Attempts = 0,
            OccurredAt = now,
            CorrelationId = correlationId
        };
    }
}

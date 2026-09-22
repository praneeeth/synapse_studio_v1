namespace DRT.Domain.Entities;

/// <summary>
/// Outbox message for downstream processing (notifications, integrations).
/// Event type: CoreAskCreated/v1
/// </summary>
public sealed class OutboxMessage
{
    public Guid MessageId { get; private set; }
    public string MessageType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTimeOffset OccurredUtc { get; private set; }
    public string? CorrelationId { get; private set; }
    public string ProcessingStatus { get; private set; } = "Pending";
    public int AttemptCount { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(string messageType, string payload, DateTimeOffset occurredUtc, string? correlationId)
    {
        return new OutboxMessage
        {
            MessageId = Guid.NewGuid(),
            MessageType = messageType,
            Payload = payload,
            OccurredUtc = occurredUtc,
            CorrelationId = correlationId,
            ProcessingStatus = "Pending",
            AttemptCount = 0
        };
    }
}

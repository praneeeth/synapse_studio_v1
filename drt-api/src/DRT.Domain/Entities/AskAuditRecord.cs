namespace DRT.Domain.Entities;

/// <summary>
/// Append-only audit record for ASK operations.
/// </summary>
public sealed class AskAuditRecord
{
    public int Id { get; private set; }
    public int AskId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public int ActorId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string? CorrelationId { get; private set; }

    private AskAuditRecord() { }

    public static AskAuditRecord Create(int askId, string action, int actorId, DateTimeOffset now, string? correlationId)
    {
        return new AskAuditRecord
        {
            AskId = askId,
            Action = action,
            ActorId = actorId,
            OccurredAt = now,
            CorrelationId = correlationId
        };
    }
}

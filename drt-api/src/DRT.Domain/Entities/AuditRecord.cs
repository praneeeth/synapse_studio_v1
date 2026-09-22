namespace DRT.Domain.Entities;

/// <summary>
/// Audit record for ASK operations.
/// </summary>
public sealed class AuditRecord
{
    public int AuditId { get; private set; }
    public int AskId { get; private set; }
    public int ActorId { get; private set; }
    public string Operation { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTimeOffset OccurredUtc { get; private set; }

    private AuditRecord() { }

    /// <summary>Creates a general audit record (e.g. CoreAskCreated).</summary>
    public static AuditRecord Create(
        int askId,
        int actorId,
        string operation,
        DateTimeOffset occurredUtc)
    {
        return new AuditRecord
        {
            AskId = askId,
            ActorId = actorId,
            Operation = operation,
            Description = string.Empty,
            CreatedBy = string.Empty,
            OccurredUtc = occurredUtc
        };
    }

    /// <summary>
    /// Creates a cancellation audit record with action "Core Ask Cancelled" per US-ASK-015.
    /// </summary>
    public static AuditRecord CreateCancellation(
        int askId,
        int actorId,
        string createdBy,
        string description,
        DateTimeOffset occurredUtc)
    {
        return new AuditRecord
        {
            AskId = askId,
            ActorId = actorId,
            Operation = "Core Ask Cancelled",
            Description = description,
            CreatedBy = createdBy,
            OccurredUtc = occurredUtc
        };
    }
}

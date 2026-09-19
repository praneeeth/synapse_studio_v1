namespace DRT.Domain.Entities;

/// <summary>
/// Append-only audit record for ASK operations.
/// </summary>
public sealed class AskAuditRecord
{
    public int Id { get; private set; }
    public int AskId { get; private set; }
    public string Action { get; private set; } = string.Empty;

    /// <summary>Human-readable description; populated with the cancellation comment (US-ASK-015).</summary>
    public string? Description { get; private set; }

    public int ActorId { get; private set; }

    /// <summary>String identity of creator (UPN); used when actor is identified by Entra UPN.</summary>
    public string? CreatedByIdentity { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }
    public string? CorrelationId { get; private set; }

    private AskAuditRecord() { }

    /// <summary>Creates an audit record for the original Create Core ASK flow (integer actor id).</summary>
    public static AskAuditRecord Create(
        int askId,
        string action,
        int actorId,
        DateTimeOffset now,
        string? correlationId)
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

    /// <summary>
    /// Creates a cancellation audit entry (US-ASK-015).
    /// action = "Core Ask Cancelled", description = comment value.
    /// </summary>
    public static AskAuditRecord CreateCancellation(
        int askId,
        string description,
        string createdBy,
        DateTimeOffset now)
    {
        return new AskAuditRecord
        {
            AskId = askId,
            Action = "Core Ask Cancelled",
            Description = description,
            ActorId = 0, // integer FK; identity stored in CreatedByIdentity
            CreatedByIdentity = createdBy,
            OccurredAt = now
        };
    }
}

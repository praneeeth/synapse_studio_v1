namespace DRT.Domain.Entities;

/// <summary>
/// Audit record for the create operation.
/// </summary>
public sealed class AuditRecord
{
    public int AuditId { get; private set; }
    public int AskId { get; private set; }
    public int ActorId { get; private set; }
    public string Operation { get; private set; } = string.Empty;
    public DateTimeOffset OccurredUtc { get; private set; }

    private AuditRecord() { }

    public static AuditRecord Create(int askId, int actorId, string operation, DateTimeOffset occurredUtc)
    {
        return new AuditRecord
        {
            AskId = askId,
            ActorId = actorId,
            Operation = operation,
            OccurredUtc = occurredUtc
        };
    }
}

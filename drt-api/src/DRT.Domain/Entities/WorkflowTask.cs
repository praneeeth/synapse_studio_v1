namespace DRT.Domain.Entities;

/// <summary>
/// Workflow task associated with an ASK.
/// </summary>
public sealed class WorkflowTask
{
    public int Id { get; private set; }
    public int AskId { get; private set; }
    public int StatusId { get; private set; }
    public int AssigneeId { get; private set; }
    public int Version { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public int CreatedBy { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private WorkflowTask() { }

    public static WorkflowTask Create(int askId, int statusId, int assigneeId, int actorId, DateTimeOffset now)
    {
        return new WorkflowTask
        {
            AskId = askId,
            StatusId = statusId,
            AssigneeId = assigneeId,
            Version = 1,
            CreatedAt = now,
            CreatedBy = actorId
        };
    }
}

namespace DRT.Domain.Entities;

/// <summary>
/// Workflow task entity with status and assignee.
/// </summary>
public sealed class WorkflowTask
{
    public int TaskId { get; private set; }
    public int AskId { get; private set; }
    public int StatusId { get; private set; }
    public int AssigneeId { get; private set; }
    public int Version { get; private set; }
    public DateTimeOffset CreatedUtc { get; private set; }

    private WorkflowTask() { }

    public static WorkflowTask Create(int askId, int statusId, int assigneeId, DateTimeOffset createdUtc)
    {
        return new WorkflowTask
        {
            AskId = askId,
            StatusId = statusId,
            AssigneeId = assigneeId,
            Version = 1,
            CreatedUtc = createdUtc
        };
    }
}

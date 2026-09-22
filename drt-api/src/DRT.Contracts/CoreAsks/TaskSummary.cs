namespace DRT.Contracts.CoreAsks;

public sealed class TaskSummary
{
    public int TaskId { get; init; }
    public int StatusId { get; init; }
    public int AssigneeId { get; init; }
}

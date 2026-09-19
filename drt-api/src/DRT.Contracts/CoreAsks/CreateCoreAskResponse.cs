namespace DRT.Contracts.CoreAsks;

public sealed class CreateCoreAskResponse
{
    public int AskId { get; init; }
    public int AskDetailId { get; init; }
    public CoreAskDetailsResponse? CoreAskDetails { get; init; }
    public int Version { get; init; }
    public TaskSummary? Task { get; init; }
    public CommentSummary? Comment { get; init; }
    public AuditSummary? AuditHistory { get; init; }
}

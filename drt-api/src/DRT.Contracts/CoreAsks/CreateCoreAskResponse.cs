namespace DRT.Contracts.CoreAsks;

/// <summary>
/// Response contract for POST /core-asks.
/// </summary>
public sealed class CreateCoreAskResponse
{
    public int AskId { get; init; }
    public int AskDetailId { get; init; }
    public CoreAskDetailsResponse CoreAskDetails { get; init; } = new();
    public int Version { get; init; }
    public TaskSummary Task { get; init; } = new();
    public CommentSummary? Comment { get; init; }
    public AuditSummary AuditHistory { get; init; } = new();
}

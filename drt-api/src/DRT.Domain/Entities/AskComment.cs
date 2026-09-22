namespace DRT.Domain.Entities;

/// <summary>
/// Optional comment associated with an ASK.
/// </summary>
public sealed class AskComment
{
    public int AskCommentId { get; private set; }
    public int AskId { get; private set; }
    public string CommentText { get; private set; } = string.Empty;
    public int CreatedByActorId { get; private set; }
    public DateTimeOffset CreatedUtc { get; private set; }

    private AskComment() { }

    public static AskComment Create(int askId, string commentText, int createdByActorId, DateTimeOffset createdUtc)
    {
        return new AskComment
        {
            AskId = askId,
            CommentText = commentText,
            CreatedByActorId = createdByActorId,
            CreatedUtc = createdUtc
        };
    }
}

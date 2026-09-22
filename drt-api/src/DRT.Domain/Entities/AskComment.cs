namespace DRT.Domain.Entities;

/// <summary>
/// Comment associated with an ASK.
/// Supports both general comments (created during ASK creation) and
/// cancellation comments (isActive=true, isDraft=false per US-ASK-015).
/// </summary>
public sealed class AskComment
{
    public int AskCommentId { get; private set; }
    public int AskId { get; private set; }

    /// <summary>Foreign key to the owning request (equals AskId for Core ASK comments).</summary>
    public int RequestId { get; private set; }

    /// <summary>
    /// Module type identifier for Core ASK.
    /// TODO: Confirm exact moduleTypeId constant for Core ASK (open item: Comment moduleTypeId for Core ASK).
    /// </summary>
    public int ModuleTypeId { get; private set; }

    public string CommentText { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public bool IsDraft { get; private set; }
    public int CreatedByActorId { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; private set; }

    private AskComment() { }

    /// <summary>Creates a general comment during ASK creation.</summary>
    public static AskComment Create(
        int askId,
        string commentText,
        int createdByActorId,
        DateTimeOffset createdUtc)
    {
        return new AskComment
        {
            AskId = askId,
            RequestId = askId,
            ModuleTypeId = 0, // TODO: confirm moduleTypeId for general comments
            CommentText = commentText,
            IsActive = true,
            IsDraft = false,
            CreatedByActorId = createdByActorId,
            CreatedBy = string.Empty,
            CreatedUtc = createdUtc
        };
    }

    /// <summary>
    /// Creates a cancellation comment (isActive=true, isDraft=false) per US-ASK-015.
    /// </summary>
    public static AskComment CreateCancellation(
        int askId,
        int requestId,
        int moduleTypeId,
        string commentText,
        int createdByActorId,
        string createdBy,
        DateTimeOffset createdUtc)
    {
        return new AskComment
        {
            AskId = askId,
            RequestId = requestId,
            ModuleTypeId = moduleTypeId,
            CommentText = commentText,
            IsActive = true,
            IsDraft = false,
            CreatedByActorId = createdByActorId,
            CreatedBy = createdBy,
            CreatedUtc = createdUtc
        };
    }
}

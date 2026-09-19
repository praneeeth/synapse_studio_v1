namespace DRT.Domain.Entities;

/// <summary>
/// Comment associated with an ASK.
/// </summary>
public sealed class AskComment
{
    public int Id { get; private set; }
    public int AskId { get; private set; }

    /// <summary>requestId maps to askId per CAW_Comment schema.</summary>
    public int RequestId { get; private set; }

    /// <summary>
    /// Module type identifier for Core ASK.
    /// TODO: US-ASK-015 - Confirm the moduleTypeId constant for Core ASK in the reference data.
    /// </summary>
    public int ModuleTypeId { get; private set; }

    public string Text { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public bool IsDraft { get; private set; }
    public int CreatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>String identity of creator (for cancellation path where identity is a UPN string).</summary>
    public string? CreatedByIdentity { get; private set; }

    private AskComment() { }

    /// <summary>Creates a comment for the original Create Core ASK flow (integer actor id).</summary>
    public static AskComment Create(int askId, string text, int actorId, DateTimeOffset now)
    {
        return new AskComment
        {
            AskId = askId,
            RequestId = askId,
            ModuleTypeId = 0, // TODO: confirm moduleTypeId for Core ASK
            Text = text,
            IsActive = true,
            IsDraft = false,
            CreatedBy = actorId,
            CreatedAt = now
        };
    }

    /// <summary>
    /// Creates a cancellation comment (US-ASK-015).
    /// isActive=true, isDraft=false per business rule.
    /// </summary>
    public static AskComment CreateForCancellation(
        int askId,
        string text,
        int moduleTypeId,
        string createdBy,
        DateTimeOffset now)
    {
        return new AskComment
        {
            AskId = askId,
            RequestId = askId,
            ModuleTypeId = moduleTypeId,
            Text = text,
            IsActive = true,
            IsDraft = false,
            CreatedBy = 0, // integer FK; identity stored in CreatedByIdentity
            CreatedByIdentity = createdBy,
            CreatedAt = now
        };
    }
}

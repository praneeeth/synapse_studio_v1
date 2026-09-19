namespace DRT.Domain.Entities;

/// <summary>
/// Links an uploaded document attachment to an ASK.
/// TODO: US-ASK-001 - Exact attachment service contract (request format, response identifiers, error handling)
/// must be confirmed before AskAttachmentLink population can be fully implemented.
/// </summary>
public sealed class AskAttachmentLink
{
    public int Id { get; private set; }
    public int AskId { get; private set; }
    /// <summary>Opaque attachment identifier returned by the attachment service.</summary>
    public string AttachmentReference { get; private set; } = string.Empty;
    public int CreatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private AskAttachmentLink() { }

    public static AskAttachmentLink Create(int askId, string attachmentReference, int actorId, DateTimeOffset now)
    {
        return new AskAttachmentLink
        {
            AskId = askId,
            AttachmentReference = attachmentReference,
            CreatedBy = actorId,
            CreatedAt = now
        };
    }
}

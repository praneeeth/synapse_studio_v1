namespace DRT.Domain.Entities;

/// <summary>
/// Links an uploaded document attachment to an ASK.
/// TODO: Confirm exact attachment identifier type from attachment upload service (open item: Attachment Upload Service Contract).
/// </summary>
public sealed class AskAttachmentLink
{
    public int AskAttachmentLinkId { get; private set; }
    public int AskId { get; private set; }
    /// <summary>Identifier returned by the attachment upload service.</summary>
    public string AttachmentIdentifier { get; private set; } = string.Empty;
    public DateTimeOffset CreatedUtc { get; private set; }

    private AskAttachmentLink() { }

    public static AskAttachmentLink Create(int askId, string attachmentIdentifier, DateTimeOffset createdUtc)
    {
        return new AskAttachmentLink
        {
            AskId = askId,
            AttachmentIdentifier = attachmentIdentifier,
            CreatedUtc = createdUtc
        };
    }
}

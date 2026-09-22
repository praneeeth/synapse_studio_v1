namespace DRT.Contracts.CoreAsks;

/// <summary>
/// Represents a single file upload payload in the documents array.
/// TODO: Confirm exact attachment upload service contract (open item: Attachment Upload Service Contract).
/// </summary>
public sealed class AttachmentPayload
{
    public string? FileName { get; init; }
    public string? ContentType { get; init; }
    /// <summary>Base64-encoded file content or reference identifier. TBD per attachment service contract.</summary>
    public string? Content { get; init; }
}

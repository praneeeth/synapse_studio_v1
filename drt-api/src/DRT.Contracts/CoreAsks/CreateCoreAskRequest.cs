namespace DRT.Contracts.CoreAsks;

/// <summary>
/// Request contract for POST /core-asks (Create Core ASK).
/// </summary>
public sealed class CreateCoreAskRequest
{
    public CoreAskDetailsRequest CoreAskDetails { get; init; } = new();
    public string? Comment { get; init; }
    /// <summary>
    /// File upload array. Exact attachment upload contract TBD.
    /// TODO: Confirm attachment upload service API contract (open item: Attachment Upload Service Contract).
    /// </summary>
    public IReadOnlyList<AttachmentPayload>? Documents { get; init; }
    /// <summary>Allowed values: "Save &amp; Exit", "SUBMIT", "Exit".</summary>
    public string ButtonValue { get; init; } = string.Empty;
}

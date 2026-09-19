namespace DRT.Contracts.CoreAsks;

/// <summary>
/// Request contract for POST /api/v1/core-asks (operationId: ASK-POST-CORE-ASKS).
/// </summary>
public sealed class CreateCoreAskRequest
{
    public CoreAskDetailsRequest CoreAskDetails { get; init; } = new();
    public string? Comment { get; init; }
    // TODO: US-ASK-001 - documents array file-upload contract is not fully defined.
    // Exact attachment service request format, response identifiers and error handling must be confirmed.
    public IReadOnlyList<string>? Documents { get; init; }
    /// <summary>Allowed values: "Save &amp; Exit" | "SUBMIT" | "Exit"</summary>
    public string ButtonValue { get; init; } = string.Empty;
}

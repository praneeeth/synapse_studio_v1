namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Read-only query for retrieving a Core ASK summary (Story 1854, US-ASK-003).
/// </summary>
public sealed class GetCoreAskSummaryQuery
{
    /// <summary>The Core ASK identifier from the route parameter.</summary>
    public int AskId { get; init; }

    /// <summary>
    /// Optional specific version number.
    /// When null, the use case returns the latest version (BR-001).
    /// </summary>
    public int? Version { get; init; }
}

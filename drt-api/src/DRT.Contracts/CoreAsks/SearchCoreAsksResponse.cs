namespace DRT.Contracts.CoreAsks;

/// <summary>
/// Response body for POST /core-asks/search (Story 1859).
/// </summary>
public sealed class SearchCoreAsksResponse
{
    /// <summary>Paginated list of Core ASK records (latest version only per askId).</summary>
    public IReadOnlyList<CoreAskSearchItem> Items { get; init; } = [];

    /// <summary>
    /// Total number of matching records across all pages.
    /// Populated only when IncludeTotalCount was true in the request.
    /// </summary>
    public int? TotalCount { get; init; }

    /// <summary>Current 1-based page number.</summary>
    public int Page { get; init; }

    /// <summary>Records per page used for this response.</summary>
    public int PageSize { get; init; }
}

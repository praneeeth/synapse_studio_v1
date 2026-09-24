namespace DRT.Contracts.CoreAsks;

/// <summary>
/// Request body for POST /core-asks/search (Story 1859).
/// </summary>
public sealed class SearchCoreAsksRequest
{
    /// <summary>Sort criteria. Defaults to askId descending when null or empty.</summary>
    public IReadOnlyList<SortCriterion>? Sort { get; init; }

    /// <summary>1-based page number. Required, must be a positive integer.</summary>
    public int Page { get; init; }

    /// <summary>Records per page. Required, must be a positive integer within the configured maximum.</summary>
    public int PageSize { get; init; }

    /// <summary>When true the response includes TotalCount.</summary>
    public bool IncludeTotalCount { get; init; }

    /// <summary>Optional allowlisted filter criteria.</summary>
    public SearchCoreAsksFilters? Filters { get; init; }
}

/// <summary>
/// A single sort criterion.
/// </summary>
public sealed class SortCriterion
{
    /// <summary>Allowlisted field name to sort by.</summary>
    public string Field { get; init; } = string.Empty;

    /// <summary>Sort direction: "asc" or "desc".</summary>
    public string Direction { get; init; } = string.Empty;
}

/// <summary>
/// Allowlisted filter fields for Core ASK search.
/// All fields are optional.
/// </summary>
public sealed class SearchCoreAsksFilters
{
    public IReadOnlyList<int>? AskDetailId { get; init; }
    public IReadOnlyList<int>? AskId { get; init; }
    public int? Version { get; init; }
    public bool? IsCompleted { get; init; }
    public IReadOnlyList<int>? FiscalYear { get; init; }
    public IReadOnlyList<string>? Status { get; init; }
    public IReadOnlyList<string>? DppGroup { get; init; }
    public IReadOnlyList<string>? Level { get; init; }
    public IReadOnlyList<string>? Speciality { get; init; }
    public IReadOnlyList<string>? Reason { get; init; }
    public IReadOnlyList<int>? DppGroupId { get; init; }
    public bool? IsActive { get; init; }
}

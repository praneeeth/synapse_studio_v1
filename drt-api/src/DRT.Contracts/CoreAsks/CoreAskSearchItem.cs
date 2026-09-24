namespace DRT.Contracts.CoreAsks;

/// <summary>
/// A single Core ASK record returned in the search result list.
/// Represents the latest version only per askId (BR-001).
/// Nullable fields display 'N/A' when null per contract specification (BR-007).
/// </summary>
public sealed class CoreAskSearchItem
{
    public int AskId { get; init; }
    public string RequestNumberDisplay { get; init; } = string.Empty;
    public string? CoreAskName { get; init; }
    public string DppGroup { get; init; } = string.Empty;

    /// <summary>Displays 'N/A' when null.</summary>
    public string LevelNeed { get; init; } = "N/A";

    /// <summary>Displays 'N/A' when null.</summary>
    public string OutGoingResource { get; init; } = "N/A";

    /// <summary>Displays 'N/A' when null.</summary>
    public string NeedReason { get; init; } = "N/A";

    public string FYear { get; init; } = string.Empty;

    /// <summary>Displays 'N/A' when null.</summary>
    public string ProjectedStartDate { get; init; } = "N/A";

    /// <summary>Displays 'N/A' when null.</summary>
    public string EndDate { get; init; } = "N/A";

    /// <summary>Displays 'N/A' when null.</summary>
    public string GeneralSpecialityNeed { get; init; } = "N/A";

    /// <summary>Displays 'N/A' when null.</summary>
    public string RolePosting { get; init; } = "N/A";

    public int Version { get; init; }
    public bool IsCompleted { get; init; }
    public int StatusId { get; init; }
}

namespace DRT.Contracts.Asks;

/// <summary>
/// Response contract for POST /api/v1/asks (ASK-POST-CORE-ASKS).
/// Returned on successful creation of a Core or Rotational ASK draft.
/// </summary>
public sealed class CreateAskResponse
{
    /// <summary>Primary key of the created ASK.</summary>
    public int AskId { get; init; }

    /// <summary>"Core" or "Rotational".</summary>
    public string AskType { get; init; } = string.Empty;

    /// <summary>Name of the ASK (Rotational only; null for Core).</summary>
    public string? NameOfTheAsk { get; init; }

    /// <summary>Calendar year of the ASK.</summary>
    public int? Year { get; init; }

    /// <summary>Status after creation. Always "Draft" on initial save.</summary>
    public string Status { get; init; } = "Draft";

    /// <summary>UTC timestamp of creation.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>DRT user ID of the creator.</summary>
    public int CreatedBy { get; init; }

    /// <summary>Per-BU plan entries (Rotational only; empty for Core).</summary>
    public IReadOnlyList<PlanByBuResponse> PlanByBU { get; init; } = Array.Empty<PlanByBuResponse>();

    /// <summary>Core ASK detail identifier (Core only; null for Rotational).</summary>
    public int? AskDetailId { get; init; }

    /// <summary>Version number of the created ASK record.</summary>
    public int Version { get; init; }
}

namespace DRT.Contracts.Asks;

/// <summary>
/// Request contract for POST /api/v1/asks (ASK-POST-CORE-ASKS).
/// Creates a Core or Rotational ASK draft.
/// </summary>
public sealed class CreateAskRequest
{
    /// <summary>
    /// Required. Discriminates the ASK type: "Core" or "Rotational".
    /// </summary>
    public string AskType { get; init; } = string.Empty;

    /// <summary>
    /// Required for Rotational ASK. Max 100 characters.
    /// Added in RT_August_Release.
    /// </summary>
    public string? NameOfTheAsk { get; init; }

    /// <summary>
    /// Required. Calendar year for the ASK.
    /// </summary>
    public int? Year { get; init; }

    /// <summary>
    /// Required for Rotational ASK. Per-BU plan details.
    /// </summary>
    public IReadOnlyList<PlanByBuRequest> PlanByBU { get; init; } = Array.Empty<PlanByBuRequest>();

    /// <summary>
    /// Core ASK-specific detail fields. Required when AskType is "Core".
    /// </summary>
    public CoreAskDetailsPayload? CoreAskDetails { get; init; }

    /// <summary>
    /// Optional comment to attach to the ASK on creation.
    /// </summary>
    public string? Comment { get; init; }

    /// <summary>
    /// Button action that triggered the submission.
    /// Accepted values: "Save &amp; Exit", "SUBMIT", "Exit".
    /// </summary>
    public string ButtonValue { get; init; } = "Save & Exit";
}

namespace DRT.Contracts.Asks;

/// <summary>
/// Per-Business-Unit plan entry for a Rotational ASK.
/// </summary>
public sealed class PlanByBuRequest
{
    /// <summary>
    /// Business Unit identifier.
    /// TODO: ASK-POST-CORE-ASKS - Full BU enumeration / validation list must be confirmed from reference data.
    /// </summary>
    public string BuId { get; init; } = string.Empty;

    /// <summary>
    /// Plan details for this BU.
    /// </summary>
    public string Plan { get; init; } = string.Empty;
}

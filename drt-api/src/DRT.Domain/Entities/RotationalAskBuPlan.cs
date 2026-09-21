namespace DRT.Domain.Entities;

/// <summary>
/// Per-Business-Unit plan entry for a Rotational ASK.
/// </summary>
public sealed class RotationalAskBuPlan
{
    public int Id { get; private set; }
    public int AskId { get; private set; }

    /// <summary>
    /// Business Unit identifier.
    /// TODO: ASK-POST-CORE-ASKS - Full BU enumeration must be confirmed from reference data.
    /// </summary>
    public string BuId { get; private set; } = string.Empty;

    /// <summary>Plan details for this BU.</summary>
    public string Plan { get; private set; } = string.Empty;

    // EF Core constructor
    private RotationalAskBuPlan() { }

    public static RotationalAskBuPlan Create(int askId, string buId, string plan)
    {
        return new RotationalAskBuPlan
        {
            AskId = askId,
            BuId = buId,
            Plan = plan
        };
    }
}

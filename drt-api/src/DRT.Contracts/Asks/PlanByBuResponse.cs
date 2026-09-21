namespace DRT.Contracts.Asks;

/// <summary>
/// Per-Business-Unit plan entry returned in <see cref="CreateAskResponse"/>.
/// </summary>
public sealed class PlanByBuResponse
{
    public int Id { get; init; }
    public string BuId { get; init; } = string.Empty;
    public string Plan { get; init; } = string.Empty;
}

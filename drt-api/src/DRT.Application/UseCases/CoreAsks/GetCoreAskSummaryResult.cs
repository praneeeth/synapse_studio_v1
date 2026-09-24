using DRT.Contracts.CoreAsks;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Result returned by the GetCoreAskSummary use case (Story 1854, US-ASK-003).
/// </summary>
public sealed class GetCoreAskSummaryResult
{
    public CoreAskSummaryResponse Response { get; init; } = new();
}

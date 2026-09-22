using DRT.Contracts.CoreAsks;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Result returned by the CancelCoreAsk use case.
/// </summary>
public sealed class CancelCoreAskResult
{
    public CancelCoreAskResponse Response { get; init; } = new();
}

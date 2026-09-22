using DRT.Contracts.CoreAsks;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Result returned by the CreateCoreAsk use case.
/// </summary>
public sealed class CreateCoreAskResult
{
    public bool IsExit { get; init; }
    public CreateCoreAskResponse? Response { get; init; }
}

using DRT.Contracts.CoreAsks;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Result returned by the Cancel Core ASK use case (US-ASK-015).
/// </summary>
public sealed class CancelCoreAskResult
{
    public bool IsSuccess { get; init; }
    public CancelCoreAskResponse? Response { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }

    public static CancelCoreAskResult Success(CancelCoreAskResponse response) =>
        new() { IsSuccess = true, Response = response };

    public static CancelCoreAskResult Failure(string errorCode, string errorMessage) =>
        new() { IsSuccess = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
}

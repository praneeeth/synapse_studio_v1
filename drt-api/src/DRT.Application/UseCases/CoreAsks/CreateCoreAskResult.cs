using DRT.Contracts.CoreAsks;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Result returned by the Create Core ASK use case.
/// </summary>
public sealed class CreateCoreAskResult
{
    public bool IsSuccess { get; init; }
    public CreateCoreAskResponse? Response { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<string> ValidationErrors { get; init; } = Array.Empty<string>();

    public static CreateCoreAskResult Success(CreateCoreAskResponse response) =>
        new() { IsSuccess = true, Response = response };

    public static CreateCoreAskResult Failure(string errorCode, string errorMessage) =>
        new() { IsSuccess = false, ErrorCode = errorCode, ErrorMessage = errorMessage };

    public static CreateCoreAskResult ValidationFailure(IReadOnlyList<string> errors) =>
        new() { IsSuccess = false, ErrorCode = "validation_failed", ValidationErrors = errors };
}

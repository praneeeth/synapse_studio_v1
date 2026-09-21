using DRT.Contracts.Asks;

namespace DRT.Application.UseCases.Asks;

/// <summary>
/// Result returned by the unified Create ASK use case (ASK-POST-CORE-ASKS).
/// </summary>
public sealed class CreateAskResult
{
    public bool IsSuccess { get; init; }
    public CreateAskResponse? Response { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<string> ValidationErrors { get; init; } = Array.Empty<string>();

    public static CreateAskResult Success(CreateAskResponse response) =>
        new() { IsSuccess = true, Response = response };

    public static CreateAskResult Failure(string errorCode, string errorMessage) =>
        new() { IsSuccess = false, ErrorCode = errorCode, ErrorMessage = errorMessage };

    public static CreateAskResult ValidationFailure(IReadOnlyList<string> errors) =>
        new() { IsSuccess = false, ErrorCode = "validation_failed", ValidationErrors = errors };
}

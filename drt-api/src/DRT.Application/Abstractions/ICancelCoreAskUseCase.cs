using DRT.Application.UseCases.CoreAsks;

namespace DRT.Application.Abstractions;

/// <summary>
/// Port for the Cancel Core ASK use case.
/// </summary>
public interface ICancelCoreAskUseCase
{
    Task<CancelCoreAskResult> ExecuteAsync(CancelCoreAskCommand command, CancellationToken cancellationToken = default);
}

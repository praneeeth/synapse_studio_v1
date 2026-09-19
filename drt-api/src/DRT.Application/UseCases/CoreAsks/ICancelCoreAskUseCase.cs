using DRT.Contracts.CoreAsks;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Port for the Cancel Core ASK use case (US-ASK-015).
/// </summary>
public interface ICancelCoreAskUseCase
{
    Task<CancelCoreAskResult> ExecuteAsync(
        int askId,
        CancelCoreAskRequest request,
        string cancelledByIdentity,
        CancellationToken cancellationToken);
}

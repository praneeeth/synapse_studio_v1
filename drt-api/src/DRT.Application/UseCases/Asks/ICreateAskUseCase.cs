using DRT.Application.UseCases.CoreAsks;
using DRT.Contracts.Asks;

namespace DRT.Application.UseCases.Asks;

/// <summary>
/// Port for the unified Create ASK use case (ASK-POST-CORE-ASKS).
/// POST /api/v1/asks — creates a Core or Rotational ASK draft.
/// </summary>
public interface ICreateAskUseCase
{
    Task<CreateAskResult> ExecuteAsync(
        CreateAskRequest request,
        CurrentActorContext actor,
        CancellationToken cancellationToken);
}

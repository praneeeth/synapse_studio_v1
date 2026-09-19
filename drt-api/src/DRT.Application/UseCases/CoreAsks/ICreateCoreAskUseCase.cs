using DRT.Contracts.CoreAsks;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Port for the Create Core ASK use case.
/// </summary>
public interface ICreateCoreAskUseCase
{
    Task<CreateCoreAskResult> ExecuteAsync(
        CreateCoreAskRequest request,
        CurrentActorContext actor,
        CancellationToken cancellationToken);
}

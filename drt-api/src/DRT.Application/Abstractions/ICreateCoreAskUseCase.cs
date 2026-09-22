using DRT.Application.UseCases.CoreAsks;

namespace DRT.Application.Abstractions;

/// <summary>
/// Port for the Create Core ASK use case.
/// </summary>
public interface ICreateCoreAskUseCase
{
    Task<CreateCoreAskResult> ExecuteAsync(CreateCoreAskCommand command, CancellationToken cancellationToken = default);
}

using DRT.Application.UseCases.CoreAsks;

namespace DRT.Application.Abstractions;

/// <summary>
/// Port for the Get Core ASK Summary use case (Story 1854, US-ASK-003).
/// </summary>
public interface IGetCoreAskSummaryUseCase
{
    Task<GetCoreAskSummaryResult> ExecuteAsync(
        GetCoreAskSummaryQuery query,
        CancellationToken cancellationToken = default);
}

using DRT.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Implements the Get Core ASK Summary use case (Story 1854, US-ASK-003).
/// Processing steps:
///   1. Validate query (done by validator before this use case is invoked).
///   2. Check whether the askId exists; return AskNotFoundException if not (ER-001).
///   3. Retrieve the summary for the requested version (or latest if version is null).
///   4. Return 404 if the askId+version combination does not exist (ER-002).
///   5. Return the mapped response (BR-001, BR-002, BR-003, BR-004).
/// Transaction boundary: read-only; no transaction required (BR-006).
/// </summary>
public sealed class GetCoreAskSummaryUseCase : IGetCoreAskSummaryUseCase
{
    private readonly ICoreAskSummaryRepository _summaryRepository;
    private readonly ILogger<GetCoreAskSummaryUseCase> _logger;

    public GetCoreAskSummaryUseCase(
        ICoreAskSummaryRepository summaryRepository,
        ILogger<GetCoreAskSummaryUseCase> logger)
    {
        _summaryRepository = summaryRepository;
        _logger = logger;
    }

    public async Task<GetCoreAskSummaryResult> ExecuteAsync(
        GetCoreAskSummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        // Step 2: Verify the askId exists to distinguish the two 404 cases
        var askExists = await _summaryRepository.AskExistsAsync(query.AskId, cancellationToken);
        if (!askExists)
        {
            _logger.LogInformation(
                "Core ASK {AskId} not found.", query.AskId);
            // Caller (controller) maps this to 404
            throw new DRT.Domain.Entities.AskNotFoundException(query.AskId);
        }

        // Step 3: Retrieve summary (latest version when query.Version is null per BR-001)
        var summary = await _summaryRepository.GetSummaryAsync(
            query.AskId, query.Version, cancellationToken);

        // Step 4: askId exists but the requested version does not
        if (summary is null)
        {
            _logger.LogInformation(
                "Core ASK {AskId} version {Version} not found.",
                query.AskId, query.Version);
            // Caller (controller) maps this to 404
            throw new DRT.Domain.Entities.AskNotFoundException(query.AskId);
        }

        // Step 5: Return mapped response
        return new GetCoreAskSummaryResult { Response = summary };
    }
}

using DRT.Contracts.CoreAsks;

namespace DRT.Application.Abstractions;

/// <summary>
/// Read-only port for retrieving Core ASK summary data (Story 1854, US-ASK-003).
/// Queries CAW_Ask and CAW_CoreAskDetail with optional version filtering.
/// TODO (OI-001): Confirm reference data lookup mechanism for statusId→status,
/// dppGroupId→dppGroup, levelNeedId→levelNeed, needReasonId→needReason.
/// </summary>
public interface ICoreAskSummaryRepository
{
    /// <summary>
    /// Returns the summary for the given askId and optional version.
    /// If version is null, returns the latest version (highest version value) per BR-001.
    /// Returns null when the askId does not exist or the askId+version combination does not exist.
    /// </summary>
    Task<CoreAskSummaryResponse?> GetSummaryAsync(
        int askId,
        int? version,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true when the given askId exists in CAW_Ask, regardless of version.
    /// Used to distinguish 404-askId-not-found from 404-version-not-found.
    /// </summary>
    Task<bool> AskExistsAsync(
        int askId,
        CancellationToken cancellationToken = default);
}

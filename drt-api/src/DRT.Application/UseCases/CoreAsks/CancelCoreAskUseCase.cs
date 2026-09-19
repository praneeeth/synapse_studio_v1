using DRT.Application.Abstractions;
using DRT.Contracts.CoreAsks;
using DRT.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Application use case: Cancel Core ASK (US-ASK-015).
/// POST /core-asks/{askId}/cancel
/// </summary>
public sealed class CancelCoreAskUseCase : ICancelCoreAskUseCase
{
    // Status constants from the design card.
    private const int StatusCancelled = 577;
    private const int StatusCompleted = 135;

    // TODO: US-ASK-015 - Confirm the moduleTypeId constant for Core ASK in the reference data.
    // The exact numeric identifier for Core ASK module type is not defined in the card.
    private const int CoreAskModuleTypeId = 0; // REPLACE with confirmed moduleTypeId

    private readonly ICoreAskRepository _repository;
    private readonly ILogger<CancelCoreAskUseCase> _logger;

    public CancelCoreAskUseCase(
        ICoreAskRepository repository,
        ILogger<CancelCoreAskUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CancelCoreAskResult> ExecuteAsync(
        int askId,
        CancelCoreAskRequest request,
        string cancelledByIdentity,
        CancellationToken cancellationToken)
    {
        // Step 2: Validate comment is present and non-empty.
        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            return CancelCoreAskResult.Failure(
                "validation_failed",
                "Comment is required for cancellation");
        }

        // Step 2b: Validate askId in body matches path parameter.
        if (request.AskId != askId)
        {
            return CancelCoreAskResult.Failure(
                "validation_failed",
                $"Request body askId {request.AskId} does not match path parameter {askId}");
        }

        // Step 3: Load Core ASK by askId.
        var ask = await _repository.GetByIdAsync(askId, cancellationToken);

        // Step 4: Validate ASK exists.
        if (ask is null)
        {
            return CancelCoreAskResult.Failure(
                "resource_not_found",
                $"Core ASK with ID {askId} not found");
        }

        // Step 5: Validate cancellable state.
        if (ask.StatusId == StatusCancelled)
        {
            return CancelCoreAskResult.Failure(
                "concurrency_conflict",
                "Core ASK is already cancelled");
        }

        if (ask.StatusId == StatusCompleted)
        {
            return CancelCoreAskResult.Failure(
                "concurrency_conflict",
                "Core ASK is already completed and cannot be cancelled");
        }

        var now = DateTimeOffset.UtcNow;

        // Step 6a: Execute domain cancellation on the aggregate.
        try
        {
            ask.Cancel(cancelledByIdentity, now);
        }
        catch (DbUpdateConcurrencyException)
        {
            return CancelCoreAskResult.Failure(
                "concurrency_conflict",
                "The ASK has been modified by another user");
        }

        // Step 6b: Create Comment record.
        var comment = AskComment.CreateForCancellation(
            askId: askId,
            text: request.Comment,
            moduleTypeId: CoreAskModuleTypeId,
            createdBy: cancelledByIdentity,
            now: now);
        await _repository.AddAskCommentAsync(comment, cancellationToken);

        // Step 6c: Create Audit Entry record.
        var audit = AskAuditRecord.CreateCancellation(
            askId: askId,
            description: request.Comment,
            createdBy: cancelledByIdentity,
            now: now);
        await _repository.AddAuditRecordAsync(audit, cancellationToken);

        // Step 7: Commit all changes in a single transaction.
        try
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            return CancelCoreAskResult.Failure(
                "concurrency_conflict",
                "The ASK has been modified by another user");
        }

        _logger.LogInformation(
            "Core ASK {AskId} cancelled by {CancelledBy} at {CancelledAt}",
            askId, cancelledByIdentity, now);

        // Step 8: Return 200 OK with response payload.
        var response = new CancelCoreAskResponse
        {
            AskId = askId,
            StatusId = StatusCancelled,
            CancelledAt = now,
            CancelledBy = cancelledByIdentity,
            AuditEntry = new CancelAuditEntry
            {
                Action = "Core Ask Cancelled",
                Description = request.Comment,
                CreatedBy = cancelledByIdentity,
                CreatedOn = now
            }
        };

        return CancelCoreAskResult.Success(response);
    }
}

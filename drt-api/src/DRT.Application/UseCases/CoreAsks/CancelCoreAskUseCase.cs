using DRT.Application.Abstractions;
using DRT.Contracts.CoreAsks;
using DRT.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Implements the Cancel Core ASK use case (US-ASK-015).
/// Processing steps:
///   1. Validate command (done by validator before this use case is invoked).
///   2. Load Core ASK by askId; return 404 if not found.
///   3. Validate cancellable state: not already Cancelled (577) or Completed (135).
///   4. Apply domain Cancel transition on the Ask aggregate.
///   5. Create AskComment record (isActive=true, isDraft=false).
///   6. Create AuditRecord (action="Core Ask Cancelled").
///   7. Persist all in a single EF Core transaction via SaveChangesAsync.
///   8. Return response payload.
/// </summary>
public sealed class CancelCoreAskUseCase : ICancelCoreAskUseCase
{
    private readonly ICoreAskRepository _coreAskRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelCoreAskUseCase> _logger;

    /// <summary>
    /// TODO: Confirm the exact moduleTypeId constant for Core ASK comments
    /// (open item: Comment moduleTypeId for Core ASK).
    /// </summary>
    private const int CoreAskModuleTypeId = 0; // TODO: replace with confirmed moduleTypeId

    public CancelCoreAskUseCase(
        ICoreAskRepository coreAskRepository,
        IAuditRepository auditRepository,
        IUnitOfWork unitOfWork,
        ILogger<CancelCoreAskUseCase> logger)
    {
        _coreAskRepository = coreAskRepository;
        _auditRepository = auditRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CancelCoreAskResult> ExecuteAsync(
        CancelCoreAskCommand command,
        CancellationToken cancellationToken = default)
    {
        // Step 2: Load the Core ASK
        var ask = await _coreAskRepository.GetByIdAsync(command.AskId, cancellationToken);
        if (ask is null)
        {
            throw new AskNotFoundException(command.AskId);
        }

        var now = DateTimeOffset.UtcNow;

        // Steps 3 & 4: Validate state and apply domain Cancel transition
        // Ask.Cancel throws AskInvalidStateException when the state is terminal.
        ask.Cancel(command.CancelledBy, command.ActorId, now);

        // Step 5: Create cancellation comment
        var comment = AskComment.CreateCancellation(
            askId: ask.AskId,
            requestId: ask.AskId,
            moduleTypeId: CoreAskModuleTypeId,
            commentText: command.Comment,
            createdByActorId: command.ActorId,
            createdBy: command.CancelledBy,
            createdUtc: now);
        await _coreAskRepository.AddAskCommentAsync(comment, cancellationToken);

        // Step 6: Create audit record
        var audit = AuditRecord.CreateCancellation(
            askId: ask.AskId,
            actorId: command.ActorId,
            createdBy: command.CancelledBy,
            description: command.Comment,
            occurredUtc: now);
        await _auditRepository.AddAuditRecordAsync(audit, cancellationToken);

        // Step 7: Persist all in a single transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Core ASK {AskId} cancelled by {CancelledBy} at {CancelledAt}",
            ask.AskId, command.CancelledBy, now);

        // Step 8: Build response
        var response = new CancelCoreAskResponse
        {
            AskId = ask.AskId,
            StatusId = ask.StatusId,
            CancelledAt = ask.CancelledAt!.Value,
            CancelledBy = ask.CancelledBy!,
            AuditEntry = new CancelAuditEntry
            {
                Action = audit.Operation,
                Description = audit.Description,
                CreatedBy = audit.CreatedBy,
                CreatedOn = audit.OccurredUtc
            }
        };

        return new CancelCoreAskResult { Response = response };
    }
}

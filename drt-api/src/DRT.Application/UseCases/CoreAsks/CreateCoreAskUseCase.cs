using DRT.Application.Abstractions;
using DRT.Contracts.CoreAsks;
using DRT.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Application use case: Create Core ASK (operationId: ASK-POST-CORE-ASKS).
/// Implements processing steps from US-ASK-001.
/// </summary>
public sealed class CreateCoreAskUseCase : ICreateCoreAskUseCase
{
    // TODO: US-ASK-001 - Confirm permission code for "Create New ASK" against the DRT RBAC matrix (Open Item - Authorization).
    private const string RequiredPermission = "CreateNewAsk";

    // TODO: US-ASK-001 - Confirm status codes 123, 127, 145 against workflow status reference data (Open Item #1).
    private const int StatusInProgress = (int)AskStatus.InProgress;
    private const int StatusPplReview = (int)AskStatus.PplReview;
    private const int StatusDppOpsReview = (int)AskStatus.DppOpsReview;

    // TODO: US-ASK-001 - Confirm the exact NeedReasonId that represents "first option" (field clearing trigger) (Open Item #8).
    // Named constant placeholder; value must be confirmed from reference data.
    private const int NeedReasonIdFirstOption = 0; // REPLACE with confirmed ID

    // TODO: US-ASK-001 - Confirm the exact NeedReasonId that represents "retirement" (endDate exemption) (Open Item #7).
    private const int NeedReasonIdRetirement = 0; // REPLACE with confirmed ID

    // TODO: US-ASK-001 - Confirm the exact GeneralSpecialityNeedId that represents "first option" (field clearing trigger) (Open Item #8).
    private const int GeneralSpecialityNeedIdFirstOption = 0; // REPLACE with confirmed ID

    // TODO: US-ASK-001 - Confirm the exact set of LevelNeedIds that require rolePostingId ("top-3 set") (Open Item #6).
    private static readonly IReadOnlySet<int> LevelNeedIdsRequiringRolePosting =
        new HashSet<int>(); // REPLACE with confirmed IDs

    // TODO: US-ASK-001 - Default assigneeId for workflow task; routing rules and assignee resolution must be confirmed.
    private const int DefaultAssigneeId = 0; // REPLACE with confirmed routing logic

    private readonly ICoreAskRepository _repository;
    private readonly ILogger<CreateCoreAskUseCase> _logger;

    public CreateCoreAskUseCase(
        ICoreAskRepository repository,
        ILogger<CreateCoreAskUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CreateCoreAskResult> ExecuteAsync(
        CreateCoreAskRequest request,
        CurrentActorContext actor,
        CancellationToken cancellationToken)
    {
        // Step 1: Confirm actor is active
        if (!actor.IsActive)
        {
            return CreateCoreAskResult.Failure("access_denied", "Actor is not an active DRT user.");
        }

        // Step 2: Authorize permission
        if (!actor.Permissions.Contains(RequiredPermission))
        {
            _logger.LogWarning("Actor {ActorId} denied Create New ASK - missing permission {Permission}",
                actor.DrtUserId, RequiredPermission);
            return CreateCoreAskResult.Failure("access_denied", "Actor does not have Create New ASK permission.");
        }

        // Step 3: Handle Exit button - no persistence
        if (request.ButtonValue == "Exit")
        {
            return CreateCoreAskResult.Success(new CreateCoreAskResponse());
        }

        // Step 4 & 5: Validate request payload and reference data
        var validationErrors = ValidateRequest(request);
        if (validationErrors.Count > 0)
        {
            return CreateCoreAskResult.ValidationFailure(validationErrors);
        }

        var details = request.CoreAskDetails;
        var now = DateTimeOffset.UtcNow;

        // Step 6: Apply conditional field rules
        int? needReasonId = details.NeedReasonId;
        int? generalSpecialityNeedId = details.GeneralSpecialityNeedId;
        string? generalSpecialityNeedComment = details.GeneralSpecialityNeedComment;
        DateOnly? projectedStartDate = details.ProjectedStartDate;
        DateOnly? endDate = details.EndDate;
        int? employeeId = details.EmployeeId;
        // outgoingResource is derived; never trusted from request
        string? outgoingResource = null;

        if (needReasonId == NeedReasonIdFirstOption)
        {
            // Clear fields per business rule
            outgoingResource = null;
            employeeId = null;
            projectedStartDate = null;
            endDate = null;
        }

        if (generalSpecialityNeedId == GeneralSpecialityNeedIdFirstOption)
        {
            generalSpecialityNeedComment = null;
        }

        // Step 7: Derive outgoing resource from employeeId
        // TODO: US-ASK-001 - Employee lookup service contract and fallback behavior must be confirmed (Open Item #4).
        // outgoingResource derivation from employeeId is a TODO pending confirmed employee lookup service.
        if (employeeId.HasValue && employeeId.Value > 0)
        {
            // TODO: inject and call employee lookup service when contract is confirmed.
            outgoingResource = null; // placeholder
        }

        // Step 8: Determine target status
        int targetStatusId;
        if (request.ButtonValue == "Save & Exit")
        {
            targetStatusId = StatusInProgress;
        }
        else // SUBMIT
        {
            // TODO: US-ASK-001 - Leadership group determination logic must be confirmed (Open Item #2).
            targetStatusId = actor.IsInLeadershipGroup ? StatusDppOpsReview : StatusPplReview;
        }

        // Step 9: Create ASK entities
        var ask = Ask.Create((AskStatus)targetStatusId, actor.DrtUserId, now);
        await _repository.AddAskAsync(ask, cancellationToken);
        // SaveChanges needed to get ask.Id - done at end in single transaction via EF tracking
        // We persist all in one SaveChangesAsync at the end.

        var askVersion = AskVersion.Create(ask.Id, 1, actor.DrtUserId, now);
        await _repository.AddAskVersionAsync(askVersion, cancellationToken);

        var coreAskDetail = CoreAskDetail.Create(
            ask.Id,
            details.CoreAskName,
            details.FYear,
            details.RegionId,
            details.DppGroupId,
            needReasonId,
            generalSpecialityNeedId,
            generalSpecialityNeedComment,
            details.LevelNeedId,
            details.Pml,
            projectedStartDate,
            endDate,
            outgoingResource,
            employeeId,
            details.HeadCountAmount,
            details.FteAmount,
            details.RolePostingId,
            details.NumberOfResources,
            details.TitlingCategory,
            details.TransitionalCoach,
            details.RoleSummary,
            details.RoleResponsibility,
            details.RoleQualification);
        await _repository.AddCoreAskDetailAsync(coreAskDetail, cancellationToken);

        AskComment? askComment = null;
        if (!string.IsNullOrWhiteSpace(request.Comment))
        {
            askComment = AskComment.Create(ask.Id, request.Comment, actor.DrtUserId, now);
            await _repository.AddAskCommentAsync(askComment, cancellationToken);
        }

        // TODO: US-ASK-001 - Attachment upload service contract must be confirmed before AskAttachmentLink population (Open Item #3).
        // Documents array handling is deferred pending confirmed attachment service contract.

        // Step 10: Create workflow task
        var workflowTask = WorkflowTask.Create(ask.Id, targetStatusId, DefaultAssigneeId, actor.DrtUserId, now);
        await _repository.AddWorkflowTaskAsync(workflowTask, cancellationToken);

        // Step 11: Persist audit and outbox in same transaction
        var audit = AskAuditRecord.Create(ask.Id, "CoreAskCreated", actor.DrtUserId, now, null);
        await _repository.AddAuditRecordAsync(audit, cancellationToken);

        var outboxPayload = System.Text.Json.JsonSerializer.Serialize(new
        {
            askId = ask.Id,
            askDetailId = coreAskDetail.Id,
            statusId = targetStatusId,
            actorId = actor.DrtUserId,
            occurredUtc = now
        });
        var outbox = OutboxMessage.Create("CoreAskCreated/v1", "Ask", ask.Id, outboxPayload, now, null);
        await _repository.AddOutboxMessageAsync(outbox, cancellationToken);

        // Single SaveChangesAsync - use case owns transaction boundary
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Core ASK {AskId} created with status {StatusId} by actor {ActorId}",
            ask.Id, targetStatusId, actor.DrtUserId);

        // Step 12: Return response
        var response = new CreateCoreAskResponse
        {
            AskId = ask.Id,
            AskDetailId = coreAskDetail.Id,
            Version = 1,
            CoreAskDetails = new CoreAskDetailsResponse
            {
                CoreAskName = coreAskDetail.CoreAskName,
                DppGroupId = coreAskDetail.DppGroupId,
                NeedReasonId = coreAskDetail.NeedReasonId,
                GeneralSpecialityNeedId = coreAskDetail.GeneralSpecialityNeedId,
                GeneralSpecialityNeedComment = coreAskDetail.GeneralSpecialityNeedComment,
                LevelNeedId = coreAskDetail.LevelNeedId,
                Pml = coreAskDetail.Pml,
                ProjectedStartDate = coreAskDetail.ProjectedStartDate,
                EndDate = coreAskDetail.EndDate,
                OutgoingResource = coreAskDetail.OutgoingResource,
                EmployeeId = coreAskDetail.EmployeeId,
                HeadCountAmount = coreAskDetail.HeadCountAmount,
                FteAmount = coreAskDetail.FteAmount,
                RolePostingId = coreAskDetail.RolePostingId,
                NumberOfResources = coreAskDetail.NumberOfResources,
                TitlingCategory = coreAskDetail.TitlingCategory,
                TransitionalCoach = coreAskDetail.TransitionalCoach,
                RoleSummary = coreAskDetail.RoleSummary,
                RoleResponsibility = coreAskDetail.RoleResponsibility,
                RoleQualification = coreAskDetail.RoleQualification
            },
            Task = new TaskSummary
            {
                TaskId = workflowTask.Id,
                StatusId = workflowTask.StatusId,
                AssigneeId = workflowTask.AssigneeId
            },
            Comment = askComment is not null ? new CommentSummary { CommentId = askComment.Id } : null,
            AuditHistory = new AuditSummary { AuditId = audit.Id }
        };

        return CreateCoreAskResult.Success(response);
    }

    private static List<string> ValidateRequest(CreateCoreAskRequest request)
    {
        var errors = new List<string>();
        var d = request.CoreAskDetails;

        // ButtonValue validation
        if (request.ButtonValue is not ("Save & Exit" or "SUBMIT" or "Exit"))
            errors.Add("buttonValue must be one of: 'Save & Exit', 'SUBMIT', 'Exit'.");

        if (request.ButtonValue == "Exit")
            return errors; // no further validation needed for Exit

        // Required fields
        if (string.IsNullOrWhiteSpace(d.CoreAskName))
            errors.Add("coreAskName is required.");
        else if (d.CoreAskName.Length > 200)
            errors.Add("coreAskName must not exceed 200 characters.");

        if (d.DppGroupId is null or 0)
            errors.Add("dppGroupId is required and must be an active option.");

        if (d.NeedReasonId is null or 0)
            errors.Add("needReasonId is required and must be an active option.");

        if (d.GeneralSpecialityNeedId is null or 0)
            errors.Add("generalSpecialityNeedId is required and must be an active option.");

        if (d.LevelNeedId is null or 0)
            errors.Add("levelNeedId is required and must be an active option.");

        if (d.HeadCountAmount is null)
            errors.Add("headCountAmount is required.");

        if (d.FteAmount is null)
            errors.Add("fteAmount is required.");

        if (string.IsNullOrWhiteSpace(d.RoleSummary))
            errors.Add("roleSummary is required.");

        if (string.IsNullOrWhiteSpace(d.RoleResponsibility))
            errors.Add("roleResponsibility is required.");

        if (string.IsNullOrWhiteSpace(d.RoleQualification))
            errors.Add("roleQualification is required.");

        if (string.IsNullOrWhiteSpace(d.TitlingCategory))
            errors.Add("titlingCategory is required.");

        // Conditional: projectedStartDate required unless needReasonId is first option
        // (first option clears it; otherwise required)
        if (d.NeedReasonId != NeedReasonIdFirstOption && d.ProjectedStartDate is null)
            errors.Add("projectedStartDate is required.");

        // Conditional: endDate required unless needReasonId is retirement or first option
        if (d.NeedReasonId != NeedReasonIdRetirement && d.NeedReasonId != NeedReasonIdFirstOption)
        {
            if (d.EndDate is null)
                errors.Add("endDate is required unless needReasonId is retirement.");
            else
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                if (d.EndDate.Value < today)
                    errors.Add("endDate must not be earlier than today.");
                if (d.ProjectedStartDate.HasValue && d.EndDate.Value <= d.ProjectedStartDate.Value)
                    errors.Add("endDate must be after projectedStartDate.");
            }
        }

        // Conditional: generalSpecialityNeedComment required unless first option
        if (d.GeneralSpecialityNeedId != GeneralSpecialityNeedIdFirstOption
            && string.IsNullOrWhiteSpace(d.GeneralSpecialityNeedComment))
        {
            errors.Add("generalSpecialityNeedComment is required for the selected generalSpecialityNeedId.");
        }

        // Conditional: rolePostingId required when levelNeedId is in top-3 set
        // TODO: US-ASK-001 - LevelNeedIdsRequiringRolePosting set is empty until confirmed (Open Item #6).
        if (d.LevelNeedId.HasValue && LevelNeedIdsRequiringRolePosting.Contains(d.LevelNeedId.Value)
            && d.RolePostingId is null or 0)
        {
            errors.Add("rolePostingId is required for the selected levelNeedId.");
        }

        // Numeric rules
        if (d.FteAmount.HasValue && d.HeadCountAmount.HasValue && d.FteAmount.Value > d.HeadCountAmount.Value)
            errors.Add("fteAmount must not exceed headCountAmount.");

        // Max length
        if (d.Pml is not null && d.Pml.Length > 99)
            errors.Add("pml must not exceed 99 characters.");

        if (d.TransitionalCoach is not null && d.TransitionalCoach.Length > 99)
            errors.Add("transitionalCoach must not exceed 99 characters.");

        return errors;
    }
}

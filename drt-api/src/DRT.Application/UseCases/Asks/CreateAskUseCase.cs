using DRT.Application.Abstractions;
using DRT.Application.UseCases.CoreAsks;
using DRT.Contracts.Asks;
using DRT.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DRT.Application.UseCases.Asks;

/// <summary>
/// Application use case: Create Core or Rotational ASK draft (ASK-POST-CORE-ASKS).
/// POST /api/v1/asks
/// </summary>
public sealed class CreateAskUseCase : ICreateAskUseCase
{
    // TODO: ASK-POST-CORE-ASKS - Confirm permission code for "Create New ASK" against the DRT RBAC matrix.
    private const string RequiredPermission = "CreateNewAsk";

    // TODO: ASK-POST-CORE-ASKS - Confirm status codes against workflow status reference data (Open Item #1).
    private const int StatusInProgress = (int)AskStatus.InProgress;
    private const int StatusPplReview = (int)AskStatus.PplReview;
    private const int StatusDppOpsReview = (int)AskStatus.DppOpsReview;

    // TODO: ASK-POST-CORE-ASKS - Confirm the NeedReasonId for "first option" from reference data (Open Item #8).
    private const int NeedReasonIdFirstOption = 0; // REPLACE with confirmed ID

    // TODO: ASK-POST-CORE-ASKS - Confirm the NeedReasonId for "retirement" from reference data (Open Item #7).
    private const int NeedReasonIdRetirement = 0; // REPLACE with confirmed ID

    // TODO: ASK-POST-CORE-ASKS - Confirm GeneralSpecialityNeedId for "first option" from reference data (Open Item #8).
    private const int GeneralSpecialityNeedIdFirstOption = 0; // REPLACE with confirmed ID

    // TODO: ASK-POST-CORE-ASKS - Confirm LevelNeedIds that require rolePostingId (top-3 set) (Open Item #6).
    private static readonly IReadOnlySet<int> LevelNeedIdsRequiringRolePosting =
        new HashSet<int>(); // REPLACE with confirmed IDs

    // TODO: ASK-POST-CORE-ASKS - Confirm default assigneeId routing logic.
    private const int DefaultAssigneeId = 0; // REPLACE with confirmed routing logic

    // TODO: ASK-POST-CORE-ASKS - Confirm moduleTypeId for Core ASK in reference data.
    private const int CoreAskModuleTypeId = 0; // REPLACE with confirmed moduleTypeId

    private readonly ICreateAskRepository _repository;
    private readonly ILogger<CreateAskUseCase> _logger;

    public CreateAskUseCase(
        ICreateAskRepository repository,
        ILogger<CreateAskUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CreateAskResult> ExecuteAsync(
        CreateAskRequest request,
        CurrentActorContext actor,
        CancellationToken cancellationToken)
    {
        // Step 1: Confirm actor is active.
        if (!actor.IsActive)
            return CreateAskResult.Failure("access_denied", "Actor is not an active DRT user.");

        // Step 2: Authorize permission.
        if (!actor.Permissions.Contains(RequiredPermission))
        {
            _logger.LogWarning(
                "Actor {ActorId} denied Create ASK - missing permission {Permission}",
                actor.DrtUserId, RequiredPermission);
            return CreateAskResult.Failure("access_denied", "Actor does not have Create New ASK permission.");
        }

        // Step 3: Handle Exit button - no persistence.
        if (request.ButtonValue == "Exit")
            return CreateAskResult.Success(new CreateAskResponse());

        // Step 4: Validate askType discriminator.
        if (string.IsNullOrWhiteSpace(request.AskType) ||
            (request.AskType != "Core" && request.AskType != "Rotational"))
        {
            return CreateAskResult.ValidationFailure(
                new[] { "askType must be \"Core\" or \"Rotational\"." });
        }

        // Step 5: Validate common and type-specific fields.
        var validationErrors = request.AskType == "Core"
            ? ValidateCoreAskRequest(request)
            : ValidateRotationalAskRequest(request);

        if (validationErrors.Count > 0)
            return CreateAskResult.ValidationFailure(validationErrors);

        var now = DateTimeOffset.UtcNow;

        // Step 6: Determine target status.
        int targetStatusId;
        if (request.ButtonValue == "Save & Exit")
        {
            targetStatusId = StatusInProgress;
        }
        else // SUBMIT
        {
            // TODO: ASK-POST-CORE-ASKS - Leadership group determination logic must be confirmed (Open Item #2).
            targetStatusId = actor.IsInLeadershipGroup ? StatusDppOpsReview : StatusPplReview;
        }

        // Step 7: Create the ASK aggregate root.
        var ask = Ask.Create((AskStatus)targetStatusId, actor.DrtUserId, now);
        await _repository.AddAskAsync(ask, cancellationToken);

        var askVersion = AskVersion.Create(ask.Id, 1, actor.DrtUserId, now);
        await _repository.AddAskVersionAsync(askVersion, cancellationToken);

        int? askDetailId = null;
        IReadOnlyList<PlanByBuResponse> planByBuResponses = Array.Empty<PlanByBuResponse>();

        if (request.AskType == "Core")
        {
            askDetailId = await CreateCoreAskDetailAsync(
                ask, request, actor, now, cancellationToken);
        }
        else
        {
            planByBuResponses = await CreateRotationalAskDetailAsync(
                ask, request, now, cancellationToken);
        }

        // Step 8: Attach optional comment.
        AskComment? askComment = null;
        if (!string.IsNullOrWhiteSpace(request.Comment))
        {
            askComment = AskComment.Create(ask.Id, request.Comment, actor.DrtUserId, now);
            await _repository.AddAskCommentAsync(askComment, cancellationToken);
        }

        // Step 9: Create workflow task.
        var workflowTask = WorkflowTask.Create(ask.Id, targetStatusId, DefaultAssigneeId, actor.DrtUserId, now);
        await _repository.AddWorkflowTaskAsync(workflowTask, cancellationToken);

        // Step 10: Audit record.
        var audit = AskAuditRecord.Create(ask.Id, "AskCreated", actor.DrtUserId, now, null);
        await _repository.AddAuditRecordAsync(audit, cancellationToken);

        // Step 11: Outbox message.
        var outboxPayload = System.Text.Json.JsonSerializer.Serialize(new
        {
            askId = ask.Id,
            askType = request.AskType,
            statusId = targetStatusId,
            actorId = actor.DrtUserId,
            occurredUtc = now
        });
        var outbox = OutboxMessage.Create("AskCreated/v1", "Ask", ask.Id, outboxPayload, now, null);
        await _repository.AddOutboxMessageAsync(outbox, cancellationToken);

        // Step 12: Single SaveChangesAsync - use case owns transaction boundary.
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "ASK {AskId} (type={AskType}) created with status {StatusId} by actor {ActorId}",
            ask.Id, request.AskType, targetStatusId, actor.DrtUserId);

        // Step 13: Build and return response.
        var statusLabel = targetStatusId == StatusInProgress ? "Draft" : "Submitted";

        var response = new CreateAskResponse
        {
            AskId = ask.Id,
            AskType = request.AskType,
            NameOfTheAsk = request.NameOfTheAsk,
            Year = request.Year,
            Status = statusLabel,
            CreatedAt = now,
            CreatedBy = actor.DrtUserId,
            PlanByBU = planByBuResponses,
            AskDetailId = askDetailId,
            Version = 1
        };

        return CreateAskResult.Success(response);
    }

    // ---------------------------------------------------------------------------
    // Private helpers
    // ---------------------------------------------------------------------------

    private async Task<int?> CreateCoreAskDetailAsync(
        Ask ask,
        CreateAskRequest request,
        CurrentActorContext actor,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var d = request.CoreAskDetails!;

        // Apply conditional field rules.
        int? needReasonId = d.NeedReasonId;
        int? generalSpecialityNeedId = d.GeneralSpecialityNeedId;
        string? generalSpecialityNeedComment = d.GeneralSpecialityNeedComment;
        DateOnly? projectedStartDate = d.ProjectedStartDate;
        DateOnly? endDate = d.EndDate;
        int? employeeId = d.EmployeeId;
        string? outgoingResource = null;

        if (needReasonId == NeedReasonIdFirstOption)
        {
            outgoingResource = null;
            employeeId = null;
            projectedStartDate = null;
            endDate = null;
        }

        if (generalSpecialityNeedId == GeneralSpecialityNeedIdFirstOption)
            generalSpecialityNeedComment = null;

        // TODO: ASK-POST-CORE-ASKS - Employee lookup service contract must be confirmed (Open Item #4).
        // outgoingResource derivation from employeeId is deferred pending confirmed employee lookup service.

        var coreAskDetail = CoreAskDetail.Create(
            ask.Id,
            d.CoreAskName,
            d.FYear,
            d.RegionId,
            d.DppGroupId,
            needReasonId,
            generalSpecialityNeedId,
            generalSpecialityNeedComment,
            d.LevelNeedId,
            d.Pml,
            projectedStartDate,
            endDate,
            outgoingResource,
            employeeId,
            d.HeadCountAmount,
            d.FteAmount,
            d.RolePostingId,
            d.NumberOfResources,
            d.TitlingCategory,
            d.TransitionalCoach,
            d.RoleSummary,
            d.RoleResponsibility,
            d.RoleQualification);

        await _repository.AddCoreAskDetailAsync(coreAskDetail, cancellationToken);
        return coreAskDetail.Id;
    }

    private async Task<IReadOnlyList<PlanByBuResponse>> CreateRotationalAskDetailAsync(
        Ask ask,
        CreateAskRequest request,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var rotationalDetail = RotationalAskDetail.Create(
            ask.Id,
            request.NameOfTheAsk,
            request.Year);
        await _repository.AddRotationalAskDetailAsync(rotationalDetail, cancellationToken);

        var responses = new List<PlanByBuResponse>();
        foreach (var planEntry in request.PlanByBU)
        {
            var buPlan = RotationalAskBuPlan.Create(ask.Id, planEntry.BuId, planEntry.Plan);
            await _repository.AddRotationalAskBuPlanAsync(buPlan, cancellationToken);
            responses.Add(new PlanByBuResponse
            {
                Id = buPlan.Id,
                BuId = buPlan.BuId,
                Plan = buPlan.Plan
            });
        }

        return responses;
    }

    // ---------------------------------------------------------------------------
    // Validation helpers
    // ---------------------------------------------------------------------------

    private static List<string> ValidateCoreAskRequest(CreateAskRequest request)
    {
        var errors = new List<string>();

        if (request.ButtonValue is not ("Save & Exit" or "SUBMIT" or "Exit"))
            errors.Add("buttonValue must be one of: 'Save & Exit', 'SUBMIT', 'Exit'.");

        if (request.ButtonValue == "Exit")
            return errors;

        if (request.CoreAskDetails is null)
        {
            errors.Add("coreAskDetails is required when askType is \"Core\".");
            return errors;
        }

        var d = request.CoreAskDetails;

        if (string.IsNullOrWhiteSpace(d.CoreAskName))
            errors.Add("coreAskDetails.coreAskName is required.");
        else if (d.CoreAskName.Length > 200)
            errors.Add("coreAskDetails.coreAskName must not exceed 200 characters.");

        if (d.DppGroupId is null or 0)
            errors.Add("coreAskDetails.dppGroupId is required.");

        if (d.NeedReasonId is null or 0)
            errors.Add("coreAskDetails.needReasonId is required.");

        if (d.GeneralSpecialityNeedId is null or 0)
            errors.Add("coreAskDetails.generalSpecialityNeedId is required.");

        if (d.LevelNeedId is null or 0)
            errors.Add("coreAskDetails.levelNeedId is required.");

        if (d.HeadCountAmount is null)
            errors.Add("coreAskDetails.headCountAmount is required.");

        if (d.FteAmount is null)
            errors.Add("coreAskDetails.fteAmount is required.");

        if (string.IsNullOrWhiteSpace(d.RoleSummary))
            errors.Add("coreAskDetails.roleSummary is required.");

        if (string.IsNullOrWhiteSpace(d.RoleResponsibility))
            errors.Add("coreAskDetails.roleResponsibility is required.");

        if (string.IsNullOrWhiteSpace(d.RoleQualification))
            errors.Add("coreAskDetails.roleQualification is required.");

        if (string.IsNullOrWhiteSpace(d.TitlingCategory))
            errors.Add("coreAskDetails.titlingCategory is required.");

        if (d.NeedReasonId != NeedReasonIdFirstOption && d.ProjectedStartDate is null)
            errors.Add("coreAskDetails.projectedStartDate is required.");

        if (d.NeedReasonId != NeedReasonIdRetirement && d.NeedReasonId != NeedReasonIdFirstOption)
        {
            if (d.EndDate is null)
                errors.Add("coreAskDetails.endDate is required unless needReasonId is retirement.");
            else
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                if (d.EndDate.Value < today)
                    errors.Add("coreAskDetails.endDate must not be earlier than today.");
                if (d.ProjectedStartDate.HasValue && d.EndDate.Value <= d.ProjectedStartDate.Value)
                    errors.Add("coreAskDetails.endDate must be after projectedStartDate.");
            }
        }

        if (d.GeneralSpecialityNeedId != GeneralSpecialityNeedIdFirstOption
            && string.IsNullOrWhiteSpace(d.GeneralSpecialityNeedComment))
        {
            errors.Add("coreAskDetails.generalSpecialityNeedComment is required for the selected generalSpecialityNeedId.");
        }

        // TODO: ASK-POST-CORE-ASKS - LevelNeedIdsRequiringRolePosting set is empty until confirmed (Open Item #6).
        if (d.LevelNeedId.HasValue && LevelNeedIdsRequiringRolePosting.Contains(d.LevelNeedId.Value)
            && d.RolePostingId is null or 0)
        {
            errors.Add("coreAskDetails.rolePostingId is required for the selected levelNeedId.");
        }

        if (d.FteAmount.HasValue && d.HeadCountAmount.HasValue && d.FteAmount.Value > d.HeadCountAmount.Value)
            errors.Add("coreAskDetails.fteAmount must not exceed headCountAmount.");

        if (d.Pml is not null && d.Pml.Length > 99)
            errors.Add("coreAskDetails.pml must not exceed 99 characters.");

        if (d.TransitionalCoach is not null && d.TransitionalCoach.Length > 99)
            errors.Add("coreAskDetails.transitionalCoach must not exceed 99 characters.");

        return errors;
    }

    private static List<string> ValidateRotationalAskRequest(CreateAskRequest request)
    {
        var errors = new List<string>();

        if (request.ButtonValue is not ("Save & Exit" or "SUBMIT" or "Exit"))
            errors.Add("buttonValue must be one of: 'Save & Exit', 'SUBMIT', 'Exit'.");

        if (request.ButtonValue == "Exit")
            return errors;

        // nameOfTheAsk: required for Rotational, max 100 characters.
        if (string.IsNullOrWhiteSpace(request.NameOfTheAsk))
            errors.Add("nameOfTheAsk is required for Rotational ASK.");
        else if (request.NameOfTheAsk.Length > 100)
            errors.Add("nameOfTheAsk must not exceed 100 characters.");

        // year: required.
        if (request.Year is null)
            errors.Add("year is required.");

        // planByBU: required, at least one entry, each entry must have buId and plan.
        if (request.PlanByBU is null || request.PlanByBU.Count == 0)
        {
            errors.Add("planByBU is required and must contain at least one entry for Rotational ASK.");
        }
        else
        {
            for (int i = 0; i < request.PlanByBU.Count; i++)
            {
                var entry = request.PlanByBU[i];
                if (string.IsNullOrWhiteSpace(entry.BuId))
                    errors.Add($"planByBU[{i}].buId is required.");
                if (string.IsNullOrWhiteSpace(entry.Plan))
                    errors.Add($"planByBU[{i}].plan is required.");
            }
        }

        return errors;
    }
}

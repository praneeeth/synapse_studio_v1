using System.Text.Json;
using DRT.Application.Abstractions;
using DRT.Contracts.CoreAsks;
using DRT.Domain.Entities;
using DRT.Workflow;
using Microsoft.Extensions.Logging;

namespace DRT.Application.UseCases.CoreAsks;

/// <summary>
/// Implements the Create Core ASK use case following the 7-step processing model.
/// </summary>
public sealed class CreateCoreAskUseCase : ICreateCoreAskUseCase
{
    private readonly ICoreAskRepository _coreAskRepository;
    private readonly IReferenceDataRepository _referenceDataRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateCoreAskUseCase> _logger;

    /// <summary>
    /// TODO: Confirm the exact NeedReasonId that represents "first option" (open item: First Option Identification).
    /// </summary>
    private const int NeedReasonFirstOptionId = 0; // TODO: replace with confirmed reference ID

    /// <summary>
    /// TODO: Confirm the exact NeedReasonId that represents "retirement" (open item: Retirement Need Reason ID).
    /// </summary>
    private const int NeedReasonRetirementId = 0; // TODO: replace with confirmed reference ID

    /// <summary>
    /// TODO: Confirm the exact GeneralSpecialityNeedId that represents "first option" (open item: First Option Identification).
    /// </summary>
    private const int GeneralSpecialityNeedFirstOptionId = 0; // TODO: replace with confirmed reference ID

    /// <summary>
    /// TODO: Confirm the exact set of LevelNeedIds that constitute the "top-3 set" (open item: Top-3 Level Need Set Definition).
    /// </summary>
    private static readonly IReadOnlySet<int> LevelNeedTopThreeSet = new HashSet<int>(); // TODO: populate with confirmed IDs

    /// <summary>
    /// TODO: Confirm assignee routing logic per workflow routing service (open item: Leadership Group Determination).
    /// </summary>
    private const int DefaultAssigneeId = 0; // TODO: replace with confirmed routing logic

    public CreateCoreAskUseCase(
        ICoreAskRepository coreAskRepository,
        IReferenceDataRepository referenceDataRepository,
        IEmployeeRepository employeeRepository,
        IAuditRepository auditRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateCoreAskUseCase> logger)
    {
        _coreAskRepository = coreAskRepository;
        _referenceDataRepository = referenceDataRepository;
        _employeeRepository = employeeRepository;
        _auditRepository = auditRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CreateCoreAskResult> ExecuteAsync(CreateCoreAskCommand command, CancellationToken cancellationToken = default)
    {
        // Step 3: Handle Exit – no persistence
        if (command.ButtonValue == ButtonValues.Exit)
        {
            return new CreateCoreAskResult { IsExit = true };
        }

        // Step 5: Validate reference data
        if (!await _referenceDataRepository.IsDppGroupActiveAsync(command.DppGroupId, cancellationToken))
            throw new InvalidReferenceDataException("Invalid option for DppGroupId");

        if (!await _referenceDataRepository.IsNeedReasonActiveAsync(command.NeedReasonId, cancellationToken))
            throw new InvalidReferenceDataException("Invalid option for NeedReasonId");

        if (!await _referenceDataRepository.IsGeneralSpecialityNeedActiveAsync(command.GeneralSpecialityNeedId, cancellationToken))
            throw new InvalidReferenceDataException("Invalid option for GeneralSpecialityNeedId");

        if (!await _referenceDataRepository.IsLevelNeedActiveAsync(command.LevelNeedId, cancellationToken))
            throw new InvalidReferenceDataException("Invalid option for LevelNeedId");

        if (command.RolePostingId.HasValue && LevelNeedTopThreeSet.Contains(command.LevelNeedId))
        {
            if (!await _referenceDataRepository.IsRolePostingActiveAsync(command.RolePostingId.Value, cancellationToken))
                throw new InvalidReferenceDataException("Invalid option for RolePostingId");
        }

        // Step 6: Apply conditional field clearing
        var needReasonId = command.NeedReasonId;
        var generalSpecialityNeedId = command.GeneralSpecialityNeedId;

        DateOnly? projectedStartDate = command.ProjectedStartDate;
        DateOnly? endDate = command.EndDate;
        int? employeeId = command.EmployeeId;
        string? generalSpecialityNeedComment = command.GeneralSpecialityNeedComment;

        if (needReasonId == NeedReasonFirstOptionId)
        {
            projectedStartDate = null;
            endDate = null;
            employeeId = null;
        }

        if (generalSpecialityNeedId == GeneralSpecialityNeedFirstOptionId)
        {
            generalSpecialityNeedComment = null;
        }

        // Step 7: Derive outgoing resource
        string? outgoingResource = null;
        if (employeeId.HasValue)
        {
            outgoingResource = await _employeeRepository.ResolveOutgoingResourceAsync(employeeId.Value, cancellationToken);
            // TODO: Clarify fallback behavior when employeeId does not resolve (open item: Employee Lookup Fallback Behavior).
        }

        // Step 8: Determine target status
        int targetStatusId;
        if (command.ButtonValue == ButtonValues.SaveAndExit)
        {
            targetStatusId = CoreAskWorkflowStatus.InProgress;
        }
        else // SUBMIT
        {
            targetStatusId = command.ActorIsLeadership
                ? CoreAskWorkflowStatus.DppOpsReview
                : CoreAskWorkflowStatus.PplReview;
        }

        var now = DateTimeOffset.UtcNow;

        // Step 9: Create ASK entities
        var ask = Ask.Create(command.ActorId, now);
        await _coreAskRepository.AddAskAsync(ask, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken); // flush to get AskId

        var askVersion = AskVersion.Create(ask.AskId, 1, command.ActorId, now);
        await _coreAskRepository.AddAskVersionAsync(askVersion, cancellationToken);

        var coreAskDetail = CoreAskDetail.Create(
            ask.AskId,
            command.CoreAskName,
            command.FYear,
            command.RegionId,
            command.DppGroupId,
            command.NeedReasonId,
            command.GeneralSpecialityNeedId,
            generalSpecialityNeedComment,
            command.LevelNeedId,
            command.Pml,
            projectedStartDate ?? default,
            endDate,
            outgoingResource,
            employeeId,
            command.HeadCountAmount,
            command.FteAmount,
            command.RolePostingId,
            command.NumberOfResources,
            command.TitlingCategory,
            command.TransitionalCoach,
            command.RoleSummary,
            command.RoleResponsibility,
            command.RoleQualification);
        await _coreAskRepository.AddCoreAskDetailAsync(coreAskDetail, cancellationToken);

        AskComment? askComment = null;
        if (!string.IsNullOrWhiteSpace(command.Comment))
        {
            askComment = AskComment.Create(ask.AskId, command.Comment, command.ActorId, now);
            await _coreAskRepository.AddAskCommentAsync(askComment, cancellationToken);
        }

        if (command.AttachmentIdentifiers != null)
        {
            foreach (var identifier in command.AttachmentIdentifiers)
            {
                var link = AskAttachmentLink.Create(ask.AskId, identifier, now);
                await _coreAskRepository.AddAskAttachmentLinkAsync(link, cancellationToken);
            }
        }

        // Step 10: Create workflow task
        var workflowTask = WorkflowTask.Create(ask.AskId, targetStatusId, DefaultAssigneeId, now);
        await _coreAskRepository.AddWorkflowTaskAsync(workflowTask, cancellationToken);

        // Step 11: Audit and outbox
        var auditRecord = AuditRecord.Create(ask.AskId, command.ActorId, "CoreAskCreated", now);
        await _auditRepository.AddAuditRecordAsync(auditRecord, cancellationToken);

        var outboxPayload = JsonSerializer.Serialize(new
        {
            askId = ask.AskId,
            askDetailId = coreAskDetail.CoreAskDetailId,
            statusId = targetStatusId,
            actorId = command.ActorId,
            occurredUtc = now
        });
        var outboxMessage = OutboxMessage.Create("CoreAskCreated/v1", outboxPayload, now, null);
        await _outboxRepository.AddOutboxMessageAsync(outboxMessage, cancellationToken);

        // Persist all in single transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Step 12: Build response
        var response = new CreateCoreAskResponse
        {
            AskId = ask.AskId,
            AskDetailId = coreAskDetail.CoreAskDetailId,
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
                TaskId = workflowTask.TaskId,
                StatusId = workflowTask.StatusId,
                AssigneeId = workflowTask.AssigneeId
            },
            Comment = askComment != null ? new CommentSummary { CommentId = askComment.AskCommentId } : null,
            AuditHistory = new AuditSummary { AuditId = auditRecord.AuditId }
        };

        return new CreateCoreAskResult { IsExit = false, Response = response };
    }
}

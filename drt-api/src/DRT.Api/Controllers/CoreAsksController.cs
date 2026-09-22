using DRT.Application.Abstractions;
using DRT.Application.UseCases.CoreAsks;
using DRT.Contracts.CoreAsks;
using DRT.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DRT.Api.Controllers;

/// <summary>
/// Endpoints for Core ASK operations.
/// POST /core-asks          – Create a new Core ASK.
/// POST /core-asks/{askId}/cancel – Cancel an existing Core ASK (US-ASK-015).
/// </summary>
[ApiController]
[Route("core-asks")]
[Authorize]
public sealed class CoreAsksController : ControllerBase
{
    private readonly ICreateCoreAskUseCase _createCoreAskUseCase;
    private readonly ICancelCoreAskUseCase _cancelCoreAskUseCase;
    private readonly IActorResolver _actorResolver;
    private readonly ILogger<CoreAsksController> _logger;

    public CoreAsksController(
        ICreateCoreAskUseCase createCoreAskUseCase,
        ICancelCoreAskUseCase cancelCoreAskUseCase,
        IActorResolver actorResolver,
        ILogger<CoreAsksController> logger)
    {
        _createCoreAskUseCase = createCoreAskUseCase;
        _cancelCoreAskUseCase = cancelCoreAskUseCase;
        _actorResolver = actorResolver;
        _logger = logger;
    }

    /// <summary>
    /// Creates and saves or submits a new Core ASK.
    /// </summary>
    /// <remarks>
    /// TODO: Confirm permission code for "Create New ASK" capability (open item: Authorization Permission Code).
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(CreateCoreAskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCoreAskAsync(
        [FromBody] CreateCoreAskRequest request,
        CancellationToken cancellationToken)
    {
        var actor = await _actorResolver.ResolveAsync(cancellationToken);
        if (actor == null)
            return Unauthorized();

        // TODO: Verify actor has "Create New ASK" permission for target BU scope.
        // The exact permission code is not defined in the card (open item: Authorization Permission Code).
        var hasPermission = actor.Roles.Count > 0; // TODO: replace with confirmed permission check
        if (!hasPermission)
            return Forbid();

        var command = new CreateCoreAskCommand
        {
            ActorId = actor.ActorId,
            ActorIsLeadership = actor.IsLeadership,
            CoreAskName = request.CoreAskDetails.CoreAskName ?? string.Empty,
            FYear = request.CoreAskDetails.FYear,
            RegionId = request.CoreAskDetails.RegionId,
            DppGroupId = request.CoreAskDetails.DppGroupId,
            NeedReasonId = request.CoreAskDetails.NeedReasonId,
            GeneralSpecialityNeedId = request.CoreAskDetails.GeneralSpecialityNeedId,
            GeneralSpecialityNeedComment = request.CoreAskDetails.GeneralSpecialityNeedComment,
            LevelNeedId = request.CoreAskDetails.LevelNeedId,
            Pml = request.CoreAskDetails.Pml,
            ProjectedStartDate = request.CoreAskDetails.ProjectedStartDate,
            EndDate = request.CoreAskDetails.EndDate,
            EmployeeId = request.CoreAskDetails.EmployeeId,
            HeadCountAmount = request.CoreAskDetails.HeadCountAmount,
            FteAmount = request.CoreAskDetails.FteAmount,
            RolePostingId = request.CoreAskDetails.RolePostingId,
            NumberOfResources = request.CoreAskDetails.NumberOfResources,
            TitlingCategory = request.CoreAskDetails.TitlingCategory,
            TransitionalCoach = request.CoreAskDetails.TransitionalCoach,
            RoleSummary = request.CoreAskDetails.RoleSummary ?? string.Empty,
            RoleResponsibility = request.CoreAskDetails.RoleResponsibility ?? string.Empty,
            RoleQualification = request.CoreAskDetails.RoleQualification ?? string.Empty,
            Comment = request.Comment,
            AttachmentIdentifiers = request.Documents?.Select(d => d.Content ?? string.Empty).ToList(),
            ButtonValue = request.ButtonValue
        };

        try
        {
            var result = await _createCoreAskUseCase.ExecuteAsync(command, cancellationToken);

            if (result.IsExit)
                return Ok();

            return CreatedAtAction(nameof(CreateCoreAskAsync), result.Response);
        }
        catch (InvalidReferenceDataException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating Core ASK");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Cancels an existing Core ASK with a mandatory comment and audit trail.
    /// Only users in the DPP Operations Editor role may cancel a Core ASK.
    /// </summary>
    /// <remarks>
    /// TODO: Replace the role-name string "DPPOperationsEditor" with the confirmed
    /// permission/policy name from the approved RBAC matrix (open item: DPP Operations Editor permission code).
    /// </remarks>
    [HttpPost("{askId:int}/cancel")]
    [ProducesResponseType(typeof(CancelCoreAskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelCoreAskAsync(
        [FromRoute] int askId,
        [FromBody] CancelCoreAskRequest request,
        CancellationToken cancellationToken)
    {
        // Step 1: Resolve authenticated actor
        var actor = await _actorResolver.ResolveAsync(cancellationToken);
        if (actor == null)
        {
            return Unauthorized(new
            {
                status = 401,
                errorCode = "authentication_required",
                message = "Authentication is required."
            });
        }

        // Step 2: Authorize – caller must be in DPP Operations Editor role
        // TODO: Replace role-name check with confirmed permission policy from RBAC matrix
        // (open item: DPP Operations Editor permission code).
        var isDppOpsEditor = actor.Roles.Contains("DPPOperationsEditor"); // TODO: confirm role name
        if (!isDppOpsEditor)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                status = 403,
                errorCode = "access_denied",
                message = "You do not have permission to cancel Core ASKs."
            });
        }

        // Step 3: Validate request body
        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            return BadRequest(new
            {
                status = 400,
                errorCode = "validation_failed",
                message = "Comment is required for cancellation."
            });
        }

        var command = new CancelCoreAskCommand
        {
            AskId = askId,
            Comment = request.Comment,
            CancelledBy = actor.UserId,
            ActorId = actor.ActorId
        };

        try
        {
            var result = await _cancelCoreAskUseCase.ExecuteAsync(command, cancellationToken);
            return Ok(result.Response);
        }
        catch (AskNotFoundException)
        {
            return NotFound(new
            {
                status = 404,
                errorCode = "resource_not_found",
                message = $"Core ASK with ID {askId} not found."
            });
        }
        catch (AskInvalidStateException ex)
        {
            return Conflict(new
            {
                status = 409,
                errorCode = "concurrency_conflict",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error cancelling Core ASK {AskId}", askId);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                status = 500,
                errorCode = "unexpected_error",
                message = "An unexpected error occurred."
            });
        }
    }
}

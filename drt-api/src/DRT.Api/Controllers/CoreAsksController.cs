using DRT.Application.Abstractions;
using DRT.Application.UseCases.CoreAsks;
using DRT.Contracts.CoreAsks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DRT.Api.Controllers;

/// <summary>
/// POST /core-asks – Create a new Core ASK.
/// </summary>
[ApiController]
[Route("core-asks")]
[Authorize]
public sealed class CoreAsksController : ControllerBase
{
    private readonly ICreateCoreAskUseCase _createCoreAskUseCase;
    private readonly IActorResolver _actorResolver;
    private readonly ILogger<CoreAsksController> _logger;

    public CoreAsksController(
        ICreateCoreAskUseCase createCoreAskUseCase,
        IActorResolver actorResolver,
        ILogger<CoreAsksController> logger)
    {
        _createCoreAskUseCase = createCoreAskUseCase;
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
        // Step 1 & 2: Resolve and authorize actor
        var actor = await _actorResolver.ResolveAsync(cancellationToken);
        if (actor == null)
            return Unauthorized();

        // TODO: Verify actor has "Create New ASK" permission for target BU scope.
        // The exact permission code is not defined in the card (open item: Authorization Permission Code).
        // Replace the placeholder below with the confirmed permission check.
        var hasPermission = actor.Roles.Count > 0; // TODO: replace with confirmed permission check
        if (!hasPermission)
            return Forbid();

        // Map request to command
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
            // TODO: Attachment identifiers should be resolved from the attachment upload service before this call.
            // The documents array contract is TBD (open item: Attachment Upload Service Contract).
            AttachmentIdentifiers = request.Documents?.Select(d => d.Content ?? string.Empty).ToList(),
            ButtonValue = request.ButtonValue
        };

        try
        {
            var result = await _createCoreAskUseCase.ExecuteAsync(command, cancellationToken);

            if (result.IsExit)
                return Ok(); // Exit: no persistence, return success

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
}

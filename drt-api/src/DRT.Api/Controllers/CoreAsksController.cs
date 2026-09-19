using DRT.Application.UseCases.CoreAsks;
using DRT.Contracts.CoreAsks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DRT.Api.Controllers;

[ApiController]
[Route("api/v1/core-asks")]
[Authorize]
public sealed class CoreAsksController : ControllerBase
{
    private readonly ICreateCoreAskUseCase _createCoreAskUseCase;
    private readonly ILogger<CoreAsksController> _logger;

    public CoreAsksController(
        ICreateCoreAskUseCase createCoreAskUseCase,
        ILogger<CoreAsksController> logger)
    {
        _createCoreAskUseCase = createCoreAskUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Create a new Core ASK draft or submit for review.
    /// operationId: ASK-POST-CORE-ASKS
    /// </summary>
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
        // Resolve actor from validated JWT claims
        // TODO: US-ASK-001 - Full actor resolution (roles, permissions, BU scope, leadership group)
        // requires the DRT authorization service. Placeholder resolution used until confirmed.
        var actorContext = ResolveActor();
        if (actorContext is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Authentication required",
                Detail = "Unable to resolve DRT actor from token.",
                Status = StatusCodes.Status401Unauthorized,
                Extensions = { ["code"] = "authentication_required" }
            });
        }

        var result = await _createCoreAskUseCase.ExecuteAsync(request, actorContext, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "access_denied" => StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
                {
                    Title = "Access denied",
                    Detail = result.ErrorMessage,
                    Status = StatusCodes.Status403Forbidden,
                    Extensions = { ["code"] = "access_denied" }
                }),
                "validation_failed" => BadRequest(new ValidationProblemDetails
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status400BadRequest,
                    Extensions =
                    {
                        ["code"] = "validation_failed",
                        ["errors"] = result.ValidationErrors
                    }
                }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Unexpected error",
                    Status = StatusCodes.Status500InternalServerError,
                    Extensions = { ["code"] = "unexpected_error" }
                })
            };
        }

        // Exit button returns 201 with empty body (no persistence)
        if (request.ButtonValue == "Exit")
        {
            return StatusCode(StatusCodes.Status201Created, result.Response);
        }

        return StatusCode(StatusCodes.Status201Created, result.Response);
    }

    private CurrentActorContext? ResolveActor()
    {
        // Actor identity derived only from validated JWT claims - never from request body.
        // TODO: US-ASK-001 - Replace with full DRT actor resolution service that loads
        // roles, permissions, BU scope and leadership group from Azure SQL.
        var userIdClaim = User.FindFirstValue("drt_user_id");
        if (!int.TryParse(userIdClaim, out var drtUserId))
            return null;

        return new CurrentActorContext
        {
            DrtUserId = drtUserId,
            IsActive = true, // TODO: resolve from DRT authorization store
            Permissions = new[] { "CreateNewAsk" }, // TODO: resolve from DRT authorization store
            IsInLeadershipGroup = false // TODO: resolve from DRT authorization store (Open Item #2)
        };
    }
}

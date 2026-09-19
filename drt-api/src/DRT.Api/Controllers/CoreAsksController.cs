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
    private readonly ICancelCoreAskUseCase _cancelCoreAskUseCase;
    private readonly ILogger<CoreAsksController> _logger;

    public CoreAsksController(
        ICreateCoreAskUseCase createCoreAskUseCase,
        ICancelCoreAskUseCase cancelCoreAskUseCase,
        ILogger<CoreAsksController> logger)
    {
        _createCoreAskUseCase = createCoreAskUseCase;
        _cancelCoreAskUseCase = cancelCoreAskUseCase;
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

        return StatusCode(StatusCodes.Status201Created, result.Response);
    }

    /// <summary>
    /// Cancel a Core ASK with mandatory comment and audit trail.
    /// operationId: US-ASK-015
    /// </summary>
    [HttpPost("{askId:int}/cancel")]
    // TODO: US-ASK-015 - Confirm the exact authorization policy name for DPP Operations Editor group
    // membership in the DRT RBAC matrix. Placeholder policy name used until confirmed.
    [Authorize(Policy = "DppOperationsEditor")] // TODO: confirm policy name against approved RBAC rules
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
        // Resolve caller identity from validated JWT claims.
        var callerIdentity = ResolveCallerIdentity();
        if (callerIdentity is null)
        {
            return Unauthorized(new
            {
                status = StatusCodes.Status401Unauthorized,
                errorCode = "authentication_required",
                message = "Authentication is required",
                traceId = HttpContext.TraceIdentifier
            });
        }

        var result = await _cancelCoreAskUseCase.ExecuteAsync(
            askId,
            request,
            callerIdentity,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "validation_failed" => BadRequest(new
                {
                    status = StatusCodes.Status400BadRequest,
                    errorCode = "validation_failed",
                    message = result.ErrorMessage,
                    traceId = HttpContext.TraceIdentifier
                }),
                "resource_not_found" => NotFound(new
                {
                    status = StatusCodes.Status404NotFound,
                    errorCode = "resource_not_found",
                    message = result.ErrorMessage,
                    traceId = HttpContext.TraceIdentifier
                }),
                "concurrency_conflict" => Conflict(new
                {
                    status = StatusCodes.Status409Conflict,
                    errorCode = "concurrency_conflict",
                    message = result.ErrorMessage,
                    traceId = HttpContext.TraceIdentifier
                }),
                "access_denied" => StatusCode(StatusCodes.Status403Forbidden, new
                {
                    status = StatusCodes.Status403Forbidden,
                    errorCode = "access_denied",
                    message = result.ErrorMessage,
                    traceId = HttpContext.TraceIdentifier
                }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    status = StatusCodes.Status500InternalServerError,
                    errorCode = "unexpected_error",
                    message = "An unexpected error occurred",
                    traceId = HttpContext.TraceIdentifier
                })
            };
        }

        return Ok(result.Response);
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

    private string? ResolveCallerIdentity()
    {
        // Resolve caller UPN or object ID from validated JWT claims.
        // Prefer UPN (upn claim), fall back to name identifier.
        return User.FindFirstValue("upn")
            ?? User.FindFirstValue(ClaimTypes.Upn)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
    }
}

using DRT.Application.UseCases.Asks;
using DRT.Application.UseCases.CoreAsks;
using DRT.Contracts.Asks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DRT.Api.Controllers;

/// <summary>
/// Handles unified ASK creation for Core and Rotational types.
/// operationId: ASK-POST-CORE-ASKS
/// Route: POST /api/v1/asks
/// </summary>
[ApiController]
[Route("api/v1/asks")]
[Authorize]
public sealed class AsksController : ControllerBase
{
    private readonly ICreateAskUseCase _createAskUseCase;
    private readonly ILogger<AsksController> _logger;

    public AsksController(
        ICreateAskUseCase createAskUseCase,
        ILogger<AsksController> logger)
    {
        _createAskUseCase = createAskUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Create a Core or Rotational ASK draft.
    /// operationId: ASK-POST-CORE-ASKS
    /// Only DPP Operators are authorized to create a new ASK.
    /// </summary>
    /// <remarks>
    /// TODO: ASK-POST-CORE-ASKS - Confirm the exact authorization policy / group claim for DPP Operator
    /// in the DRT RBAC matrix. The policy below is a placeholder until the approved claim is confirmed.
    /// </remarks>
    [HttpPost]
    // TODO: ASK-POST-CORE-ASKS - Replace "DppOperationsEditor" with the confirmed policy name for DPP Operator role.
    [Authorize(Policy = "DppOperationsEditor")]
    [ProducesResponseType(typeof(CreateAskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAskAsync(
        [FromBody] CreateAskRequest request,
        CancellationToken cancellationToken)
    {
        // Resolve actor from validated JWT claims.
        // TODO: ASK-POST-CORE-ASKS - Replace with full DRT actor resolution service when confirmed.
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

        var result = await _createAskUseCase.ExecuteAsync(request, actorContext, cancellationToken);

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
                "validation_failed" => UnprocessableEntity(new ValidationProblemDetails
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status422UnprocessableEntity,
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

        // 201 Created per card (create operation).
        return StatusCode(StatusCodes.Status201Created, result.Response);
    }

    private CurrentActorContext? ResolveActor()
    {
        // Actor identity derived only from validated JWT claims - never from request body.
        // TODO: ASK-POST-CORE-ASKS - Replace with full DRT actor resolution service that loads
        // roles, permissions, BU scope and leadership group from Azure SQL.
        var userIdClaim = User.FindFirstValue("drt_user_id");
        if (!int.TryParse(userIdClaim, out var drtUserId))
            return null;

        return new CurrentActorContext
        {
            DrtUserId = drtUserId,
            IsActive = true,                          // TODO: resolve from DRT authorization store
            Permissions = new[] { "CreateNewAsk" },   // TODO: resolve from DRT authorization store
            IsInLeadershipGroup = false               // TODO: resolve from DRT authorization store (Open Item #2)
        };
    }
}

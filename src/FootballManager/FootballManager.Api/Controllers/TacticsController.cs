using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.Tactics;
using FootballManager.Application.Services;
using FootballManager.Domain.Tactics;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class TacticsController(ITacticsService tacticsService) : ControllerBase
{
    /// <summary>
    /// The user's three tactic slots with the game's tactical effectiveness ratings, and the active tactic in full,
    /// read live from the running game.
    /// </summary>
    [HttpGet]
    public async Task<Results<Ok<TeamTacticsResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await tacticsService.GetTacticsAsync(cancellationToken);
        return result.ToGetResult<TeamTactics, TeamTacticsResponse>(t => t.Adapt<TeamTacticsResponse>());
    }

    /// <summary>Every role available at a position, with the squad players best rated in each.</summary>
    /// <param name="position">FM's abbreviation: GK, SW, DR, DL, DC, WBR, WBL, DM, MR, ML, MC, AMR, AML, AMC or ST.</param>
    /// <param name="inPossession">The in-possession roles, or the out-of-possession ones.</param>
    /// <param name="top">How many players to list per role, best first.</param>
    [HttpGet("role-fit")]
    public async Task<Results<Ok<RoleFitResponse>, BadRequest, NotFound, ProblemHttpResult>> GetRoleFitAsync(
        [FromQuery] string position,
        [FromQuery] bool inPossession = true,
        [FromQuery] int top = TacticsService.DefaultPlayersPerRole,
        CancellationToken cancellationToken = default)
    {
        var result = await tacticsService.GetRoleFitAsync(position, inPossession, top, cancellationToken);
        return result.ToGetResult<RoleFit, RoleFitResponse>(f => f.Adapt<RoleFitResponse>());
    }
}

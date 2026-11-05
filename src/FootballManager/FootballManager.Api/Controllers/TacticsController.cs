using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Requests.Tactics.Editing;
using FootballManager.Api.Responses.Tactics;
using FootballManager.Api.Responses.Tactics.Editing;
using FootballManager.Application.Services;
using FootballManager.Domain.Tactics;
using FootballManager.Domain.Tactics.Editing;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class TacticsController(ITacticsService tacticsService, ITacticEditingService tacticEditingService) : ControllerBase
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

    /// <summary>
    /// Everything a tactic can be set to, by key (the game's English identifiers) with the game's labels: styles,
    /// mentalities, each phase's formations, team instructions with the active tactic's options, player instruction
    /// groups and role keys.
    /// </summary>
    [HttpGet("options")]
    public async Task<Results<Ok<TacticOptionsResponse>, BadRequest, NotFound, ProblemHttpResult>> GetOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await tacticEditingService.GetOptionsAsync(cancellationToken);
        return result.ToGetResult<TacticOptions, TacticOptionsResponse>(o => o.Adapt<TacticOptionsResponse>());
    }

    /// <summary>
    /// Changes a tactic slot: checks the changes and plans the game's steps, and with <paramref name="apply"/> writes
    /// them, the slot becoming the one the team plays.
    /// </summary>
    /// <param name="request">What to change, by key; anything left out stays as it is.</param>
    /// <param name="apply">False (the default) changes nothing in the game; true writes, once the user has agreed.</param>
    [HttpPost]
    public async Task<Results<Ok<TacticEditResponse>, BadRequest, NotFound, ProblemHttpResult>> SetAsync(
        [FromBody] SetTacticRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await tacticEditingService.SetTacticAsync(request.Adapt<TacticChanges>(), apply, cancellationToken);
        return result.ToGetResult<TacticEdit, TacticEditResponse>(e => e.Adapt<TacticEditResponse>());
    }
}

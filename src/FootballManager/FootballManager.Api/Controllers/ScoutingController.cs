using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Requests.Scouting;
using FootballManager.Api.Responses.Scouting;
using FootballManager.Application.Services;
using FootballManager.Domain.Scouting;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class ScoutingController(
    IScoutingService scoutingService, IScoutingEditingService editingService, IPlayerSearchService searchService) : ControllerBase
{
    /// <summary>
    /// The club's scouting, read live from the running game: the scouting centre's counts, the scouts with their
    /// judging ability and potential, and every recruitment focus with its brief and how many players it has found.
    /// </summary>
    [HttpGet]
    public async Task<Results<Ok<ScoutingOverviewResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await scoutingService.GetOverviewAsync(cancellationToken);
        return result.ToGetResult<ScoutingOverview, ScoutingOverviewResponse>(o => o.Adapt<ScoutingOverviewResponse>());
    }

    /// <summary>The players of one of the user's scouting lists, as the club's scouting knows them, best graded first.</summary>
    /// <param name="source">Recommendations (the scouting centre's), Shortlist, Focus, or Search (the game's player search).</param>
    /// <param name="focusId">The recruitment focus, for the Focus source.</param>
    /// <param name="pool">For a focus: Recommended, Current (still being assessed) or NearMiss.</param>
    /// <param name="includeAttributes">Add each player's visible attributes, as ranges while knowledge is partial.</param>
    /// <param name="includeHidden">Add hidden attributes and current/potential ability (masked without the In-Game Editor).</param>
    /// <param name="includeReport">Add each player's latest scout report: pros, cons, fee, wage demands and interest.</param>
    /// <param name="limit">At most this many players (1-200); the search reads 50 by default, in its own order.</param>
    [HttpGet("players")]
    public async Task<Results<Ok<ScoutedPlayerListResponse>, BadRequest, NotFound, ProblemHttpResult>> GetPlayersAsync(
        [FromQuery] ScoutingSource source = ScoutingSource.Recommendations,
        [FromQuery] int? focusId = null,
        [FromQuery] FocusPool pool = FocusPool.Recommended,
        [FromQuery] bool includeAttributes = false,
        [FromQuery] bool includeHidden = false,
        [FromQuery] bool includeReport = false,
        [FromQuery] int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var result = await scoutingService.GetPlayersAsync(
            source, focusId, pool, includeAttributes, includeHidden, includeReport, limit, cancellationToken);

        return result.ToGetResult<ScoutedPlayerList, ScoutedPlayerListResponse>(l => l.Adapt<ScoutedPlayerListResponse>());
    }

    /// <summary>
    /// Runs the game's player search with these filters, replacing the rules and quick filters its search screen held
    /// (the interest only changes when given). Checks everything against the game first and returns what the search
    /// holds now; writes only with apply=true. The results are then read with source=Search.
    /// </summary>
    [HttpPut("search")]
    public async Task<Results<Ok<PlayerSearchChangeResponse>, BadRequest, NotFound, ProblemHttpResult>> RunSearchAsync(
        [FromBody] RunPlayerSearchRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await searchService.RunAsync(request.Adapt<PlayerSearchCriteria>(), apply, cancellationToken);
        return result.ToGetResult<PlayerSearchChange, PlayerSearchChangeResponse>(c => c.Adapt<PlayerSearchChangeResponse>());
    }

    /// <summary>
    /// Creates a recruitment focus through the game's focus wizard: its name, areas, positions, age range and minimum
    /// ability and potential stars. Checks everything against the game first; writes only with apply=true.
    /// </summary>
    [HttpPost("focuses")]
    public async Task<Results<Ok<FocusCreationResponse>, BadRequest, NotFound, ProblemHttpResult>> CreateFocusAsync(
        [FromBody] CreateFocusRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await editingService.CreateFocusAsync(request.Adapt<FocusBrief>(), apply, cancellationToken);
        return result.ToGetResult<FocusCreation, FocusCreationResponse>(c => c.Adapt<FocusCreationResponse>());
    }

    /// <summary>Removes a recruitment focus; only with apply=true.</summary>
    [HttpDelete("focuses/{focusId:int}")]
    public async Task<Results<Ok<FocusRemovalResponse>, BadRequest, NotFound, ProblemHttpResult>> RemoveFocusAsync(
        int focusId,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await editingService.RemoveFocusAsync(focusId, apply, cancellationToken);
        return result.ToGetResult<FocusRemoval, FocusRemovalResponse>(r => r.Adapt<FocusRemovalResponse>());
    }

    /// <summary>
    /// Sends a scout (named, or any) to watch a player from the user's scouting lists, through the player's menu in the
    /// game; only with apply=true.
    /// </summary>
    [HttpPost("players/{playerId:int}/scout")]
    public async Task<Results<Ok<PlayerScoutingChangeResponse>, BadRequest, NotFound, ProblemHttpResult>> ScoutPlayerAsync(
        int playerId,
        [FromBody] ScoutPlayerRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await editingService.ScoutPlayerAsync(playerId, request.Scout, request.Duration, apply, cancellationToken);
        return result.ToGetResult<PlayerScoutingChange, PlayerScoutingChangeResponse>(c => c.Adapt<PlayerScoutingChangeResponse>());
    }

    /// <summary>
    /// Sends a scout (named, or any) to watch every player of a team who isn't watched yet, one assignment each, through
    /// each player's menu in the game; only with apply=true and the dry run's count of players (expectedPlayers).
    /// </summary>
    [HttpPost("teams/{teamId:int}/scout")]
    public async Task<Results<Ok<TeamScoutingChangeResponse>, BadRequest, NotFound, ProblemHttpResult>> ScoutTeamAsync(
        int teamId,
        [FromBody] ScoutTeamRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await editingService.ScoutTeamAsync(teamId, request.Scout, request.Duration, request.ExpectedPlayers, apply, cancellationToken);
        return result.ToGetResult<TeamScoutingChange, TeamScoutingChangeResponse>(c => c.Adapt<TeamScoutingChangeResponse>());
    }

    /// <summary>Makes a new shortlist by name; only with apply=true.</summary>
    [HttpPost("shortlists")]
    public async Task<Results<Ok<ShortlistCreationResponse>, BadRequest, NotFound, ProblemHttpResult>> CreateShortlistAsync(
        [FromBody] CreateShortlistRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await editingService.CreateShortlistAsync(request.Name, apply, cancellationToken);
        return result.ToGetResult<ShortlistCreation, ShortlistCreationResponse>(c => c.Adapt<ShortlistCreationResponse>());
    }

    /// <summary>
    /// Adds a player from the user's scouting lists, the database search or a competition's stats to one of the
    /// shortlists; only with apply=true.
    /// </summary>
    [HttpPost("players/{playerId:int}/shortlist")]
    public async Task<Results<Ok<PlayerScoutingChangeResponse>, BadRequest, NotFound, ProblemHttpResult>> ShortlistPlayerAsync(
        int playerId,
        [FromBody] ShortlistPlayerRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await editingService.ShortlistPlayerAsync(playerId, request.Shortlist, request.Duration, apply, cancellationToken);
        return result.ToGetResult<PlayerScoutingChange, PlayerScoutingChangeResponse>(c => c.Adapt<PlayerScoutingChangeResponse>());
    }

    /// <summary>Takes a player off the shortlist; only with apply=true.</summary>
    [HttpDelete("players/{playerId:int}/shortlist")]
    public async Task<Results<Ok<PlayerScoutingChangeResponse>, BadRequest, NotFound, ProblemHttpResult>> UnshortlistPlayerAsync(
        int playerId,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await editingService.UnshortlistPlayerAsync(playerId, apply, cancellationToken);
        return result.ToGetResult<PlayerScoutingChange, PlayerScoutingChangeResponse>(c => c.Adapt<PlayerScoutingChangeResponse>());
    }
}

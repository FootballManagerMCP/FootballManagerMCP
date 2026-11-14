using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Requests.Competitions;
using FootballManager.Api.Responses.Competitions;
using FootballManager.Application.Services;
using FootballManager.Domain.Competitions;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class CompetitionsController(ICompetitionsService competitionsService) : ControllerBase
{
    /// <summary>
    /// The leagues whose stats screens can be read, read live from the running game: the user's, and those the game
    /// plays out in the nations of the user's continent, each with its reputation.
    /// </summary>
    [HttpGet]
    public async Task<Results<Ok<CompetitionListResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await competitionsService.GetCompetitionsAsync(cancellationToken);
        return result.ToGetResult<List<CompetitionInfo>, CompetitionListResponse>(competitions => competitions.Adapt<CompetitionListResponse>());
    }

    /// <summary>
    /// A competition's players ranked by a stat on its stats screen (the game keeps the top 100 of each), narrowed
    /// down by position, age and minutes, each with his other stats read, what the user's scouting knows of him, and
    /// his contract.
    /// </summary>
    [HttpGet("player-stats")]
    public async Task<Results<Ok<CompetitionPlayerStatsResponse>, BadRequest, NotFound, ProblemHttpResult>> GetPlayerStatsAsync(
        [FromQuery] CompetitionStatsRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await competitionsService.GetPlayerStatsAsync(request.Adapt<CompetitionStatsQuery>(), cancellationToken);
        return result.ToGetResult<CompetitionPlayerStats, CompetitionPlayerStatsResponse>(stats => stats.Adapt<CompetitionPlayerStatsResponse>());
    }
}

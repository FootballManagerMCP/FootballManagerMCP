using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.Matches;
using FootballManager.Application.Services;
using FootballManager.Domain.Matches;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class MatchesController(IMatchesService matchesService) : ControllerBase
{
    /// <summary>
    /// The user's season so far, read live from the running game: the record over competitive matches (overall, home
    /// and away), every match played and the next few.
    /// </summary>
    /// <param name="includeFriendlies">List friendlies among the matches played (they never count in the record).</param>
    [HttpGet]
    public async Task<Results<Ok<SeasonMatchesResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        [FromQuery] bool includeFriendlies = false,
        CancellationToken cancellationToken = default)
    {
        var result = await matchesService.GetMatchesAsync(includeFriendlies, cancellationToken);
        return result.ToGetResult<SeasonMatches, SeasonMatchesResponse>(m => m.Adapt<SeasonMatchesResponse>());
    }

    /// <summary>One match in depth: both teams' numbers, the goals, and the user's players with positions, roles and stats.</summary>
    /// <param name="date">The match date, yyyy-MM-dd.</param>
    [HttpGet("{date}")]
    public async Task<Results<Ok<MatchDetailResponse>, BadRequest, NotFound, ProblemHttpResult>> GetMatchAsync(
        string date,
        CancellationToken cancellationToken = default)
    {
        var result = await matchesService.GetMatchAsync(date, cancellationToken);
        return result.ToGetResult<MatchDetail, MatchDetailResponse>(m => m.Adapt<MatchDetailResponse>());
    }
}

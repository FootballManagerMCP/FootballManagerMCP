using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.Teams;
using FootballManager.Application.Services;
using FootballManager.Domain.Teams;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class TeamsController(ITeamsService teamsService) : ControllerBase
{
    /// <summary>
    /// A scouting report on any team of the user's league or competitions, read live from the running game: record,
    /// style per match (home and away, for and against), shapes and regular starters, squad, and every match.
    /// </summary>
    /// <param name="team">The team's name as the game writes it (any case, accents optional), or its FM id.</param>
    [HttpGet("{team}")]
    public async Task<Results<Ok<TeamReportResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        string team,
        CancellationToken cancellationToken = default)
    {
        var result = await teamsService.GetTeamAsync(team, cancellationToken);
        return result.ToGetResult<TeamReport, TeamReportResponse>(r => r.Adapt<TeamReportResponse>());
    }
}

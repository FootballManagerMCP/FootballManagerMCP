using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.Database;
using FootballManager.Application.Services;
using FootballManager.Domain.Database;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class DatabaseController(IDatabaseSearchService searchService) : ControllerBase
{
    /// <summary>The game's search for a text, from the closest match down, typed into the game's search box.</summary>
    /// <param name="query">A name or part of one: a player, staff member, club, competition or nation.</param>
    /// <param name="kinds">Only these kinds of results; all kinds when none are given.</param>
    /// <param name="limit">At most this many results (1-50, 10 by default).</param>
    [HttpGet("search")]
    public async Task<Results<Ok<DatabaseSearchResponse>, BadRequest, NotFound, ProblemHttpResult>> SearchAsync(
        [FromQuery] string? query,
        [FromQuery] List<DatabaseRecordKind>? kinds = null,
        [FromQuery] int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var result = await searchService.SearchAsync(query, kinds, limit, cancellationToken);
        return result.ToGetResult<DatabaseSearch, DatabaseSearchResponse>(search => search.Adapt<DatabaseSearchResponse>());
    }
}

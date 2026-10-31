using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses;
using FootballManager.Application.Services;
using FootballManager.Domain.Entities;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class SquadController(ISquadService squadService) : ControllerBase
{
    /// <summary>The user's first-team squad, read live from the running game.</summary>
    /// <param name="includeAttributes">Add each player's visible attributes.</param>
    /// <param name="includeHidden">Add hidden attributes and current/potential ability (masked without the In-Game Editor).</param>
    [HttpGet]
    public async Task<Results<Ok<SquadResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        [FromQuery] bool includeAttributes = false,
        [FromQuery] bool includeHidden = false,
        CancellationToken cancellationToken = default)
    {
        var result = await squadService.GetSquadAsync(includeAttributes, includeHidden, cancellationToken);
        return result.ToGetResult<Squad, SquadResponse>(s => s.Adapt<SquadResponse>());
    }
}

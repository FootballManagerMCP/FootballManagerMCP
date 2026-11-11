using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Requests.Trials;
using FootballManager.Api.Responses.Trials;
using FootballManager.Application.Services;
using FootballManager.Domain.Scouting;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class TrialsController(ITrialsService trialsService) : ControllerBase
{
    /// <summary>
    /// Offers each player a trial at the user's club, of 1 to 4 weeks, through his own menu in the game; only with
    /// apply=true and the dry run's count of players (expectedPlayers).
    /// </summary>
    [HttpPost]
    public async Task<Results<Ok<TrialChangesResponse>, BadRequest, NotFound, ProblemHttpResult>> OfferAsync(
        [FromBody] OfferTrialsRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await trialsService.OfferAsync(request.PlayerIds, request.Weeks, request.ExpectedPlayers, apply, cancellationToken);
        return result.ToGetResult<TrialChanges, TrialChangesResponse>(c => c.Adapt<TrialChangesResponse>());
    }

    /// <summary>
    /// Ends the trial of each player on trial at the club, and withdraws the offer to each player who hasn't answered
    /// it yet, through his own menu in the game; only with apply=true and the dry run's count (expectedPlayers).
    /// </summary>
    [HttpPost("end")]
    public async Task<Results<Ok<TrialChangesResponse>, BadRequest, NotFound, ProblemHttpResult>> EndAsync(
        [FromBody] EndTrialsRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await trialsService.EndAsync(request.PlayerIds, request.ExpectedPlayers, apply, cancellationToken);
        return result.ToGetResult<TrialChanges, TrialChangesResponse>(c => c.Adapt<TrialChangesResponse>());
    }
}

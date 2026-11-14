using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Requests.Training;
using FootballManager.Api.Responses.Training;
using FootballManager.Application.Services;
using FootballManager.Domain.Training;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class TrainingController(ITrainingService trainingService) : ControllerBase
{
    /// <summary>
    /// The team's training, read live from the running game: this week's and next week's sessions, the session types,
    /// and each player's own training (position and role, additional focus, intensity, rating, workload).
    /// </summary>
    [HttpGet]
    public async Task<Results<Ok<TrainingResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(CancellationToken cancellationToken = default)
    {
        var result = await trainingService.GetTrainingAsync(cancellationToken);
        return result.ToGetResult<TrainingOverview, TrainingResponse>(training => training.Adapt<TrainingResponse>());
    }

    /// <summary>
    /// Changes players' additional focus and intensity, and sessions of this week's or next week's schedule, as the
    /// game's training screens do; a dry run unless apply=true, each change read back once sent.
    /// </summary>
    [HttpPost]
    public async Task<Results<Ok<TrainingChangesResponse>, BadRequest, NotFound, ProblemHttpResult>> SetAsync(
        [FromBody] SetTrainingRequest request,
        [FromQuery] bool apply = false,
        CancellationToken cancellationToken = default)
    {
        var result = await trainingService.SetTrainingAsync(request.Adapt<TrainingEdits>(), apply, cancellationToken);
        return result.ToGetResult<TrainingChanges, TrainingChangesResponse>(changes => changes.Adapt<TrainingChangesResponse>());
    }
}

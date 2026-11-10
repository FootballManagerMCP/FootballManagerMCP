using Asp.Versioning;
using FootballManager.Api.Extensions;
using FootballManager.Api.Responses.Messages;
using FootballManager.Application.Services;
using FootballManager.Domain.Messages;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FootballManager.Api.Controllers;

[ApiController]
[ApiVersion(Versions.V1)]
[Route("api/v{version:apiVersion}/[controller]")]
public class InboxController(IInboxService inboxService) : ControllerBase
{
    /// <summary>The user's inbox, read live from the running game, newest first.</summary>
    /// <param name="unreadOnly">Only the unread messages.</param>
    /// <param name="limit">At most this many messages (1-100, 20 by default).</param>
    /// <param name="skip">Start after this many, for older pages.</param>
    /// <param name="includeArticles">Add the text of the press articles staff forward.</param>
    [HttpGet]
    public async Task<Results<Ok<InboxResponse>, BadRequest, NotFound, ProblemHttpResult>> GetAsync(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int? limit = null,
        [FromQuery] int skip = 0,
        [FromQuery] bool includeArticles = false,
        CancellationToken cancellationToken = default)
    {
        var result = await inboxService.GetInboxAsync(unreadOnly, limit, skip, includeArticles, cancellationToken);
        return result.ToGetResult<Inbox, InboxResponse>(inbox => inbox.Adapt<InboxResponse>());
    }
}

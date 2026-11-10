using FluentResults;
using FootballManager.Application.Exceptions;
using FootballManager.Application.Interfaces;
using FootballManager.Application.ResultErrors;
using FootballManager.Domain.Messages;

namespace FootballManager.Application.Services;

public interface IInboxService
{
    /// <summary>A page of the user's inbox, newest first.</summary>
    Task<Result<Inbox>> GetInboxAsync(
        bool unreadOnly = false, int? limit = null, int skip = 0, bool includeArticles = false, CancellationToken cancellationToken = default);
}

public class InboxService(IInboxGameClient gameClient) : IInboxService
{
    public const int DefaultLimit = 20;
    public const int MaxLimit = 100;

    public async Task<Result<Inbox>> GetInboxAsync(
        bool unreadOnly = false, int? limit = null, int skip = 0, bool includeArticles = false, CancellationToken cancellationToken = default)
    {
        if (limit is < 1 or > MaxLimit)
        {
            return Result.Fail<Inbox>(new ValidationError("limit", [$"The limit runs 1 to {MaxLimit}."]));
        }

        if (skip < 0)
        {
            return Result.Fail<Inbox>(new ValidationError("skip", ["Skip can't be negative."]));
        }

        try
        {
            return Result.Ok(await gameClient.GetInboxAsync(unreadOnly, skip, limit ?? DefaultLimit, includeArticles, cancellationToken));
        }
        catch (GameUnavailableException exception)
        {
            return Result.Fail<Inbox>(new GameUnavailableError(exception.Message));
        }
    }
}

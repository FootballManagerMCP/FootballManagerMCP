using FootballManager.Domain.Messages;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// Reads the user's inbox in the running game. Throws <see cref="Exceptions.GameUnavailableException"/> when the game
/// can't be reached.
/// </summary>
public interface IInboxGameClient
{
    /// <summary>
    /// The messages from <paramref name="skip"/> on, at most <paramref name="take"/>: the whole inbox, or only the
    /// unread messages. A forwarded article's text comes only with <paramref name="includeArticles"/>.
    /// </summary>
    Task<Inbox> GetInboxAsync(bool unreadOnly, int skip, int take, bool includeArticles, CancellationToken cancellationToken = default);
}

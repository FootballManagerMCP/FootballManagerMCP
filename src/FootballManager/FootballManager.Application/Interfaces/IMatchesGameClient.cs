using FootballManager.Domain.Matches;

namespace FootballManager.Application.Interfaces;

/// <summary>Reads the user's matches from the running game. Throws <see cref="Exceptions.GameUnavailableException"/> when it can't.</summary>
public interface IMatchesGameClient
{
    /// <summary>Returns every fixture of the user's team this season, played and the next few, friendlies included.</summary>
    Task<TeamFixtures> GetFixturesAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the user's match on <paramref name="date"/> in depth, or null if there's none that day.</summary>
    Task<MatchDetail?> GetMatchAsync(DateOnly date, CancellationToken cancellationToken = default);
}

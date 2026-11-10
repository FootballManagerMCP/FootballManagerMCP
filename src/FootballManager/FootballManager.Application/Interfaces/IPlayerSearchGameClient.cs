using FootballManager.Domain.Scouting;

namespace FootballManager.Application.Interfaces;

/// <summary>
/// The game's player search (the search screen over the whole database): what it filters on, and running it with
/// other filters. Throws <see cref="Exceptions.GameUnavailableException"/> when the game can't be reached, and
/// <see cref="Exceptions.GameRejectedException"/> when it refuses or doesn't take a change.
/// </summary>
public interface IPlayerSearchGameClient
{
    Task<PlayerSearchFilters> GetFiltersAsync(CancellationToken cancellationToken = default);

    /// <summary>What the search's rules can take.</summary>
    Task<PlayerSearchChoices> GetChoicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the search's rules and quick filters with <paramref name="criteria"/>, sets its interest when given, and
    /// runs it. <paramref name="current"/> is what it held, put back should the game refuse part of it.
    /// </summary>
    Task RunAsync(PlayerSearchCriteria criteria, PlayerSearchFilters current, CancellationToken cancellationToken = default);
}

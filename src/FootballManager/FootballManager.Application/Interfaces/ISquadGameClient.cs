using FootballManager.Domain.Players;

namespace FootballManager.Application.Interfaces;

/// <summary>Reads the squad from the running game. Throws <see cref="Exceptions.GameUnavailableException"/> when it can't.</summary>
public interface ISquadGameClient
{
    /// <summary>
    /// Returns the user's first-team squad with the given <paramref name="attributes"/> for each player, plus current
    /// and potential ability when <paramref name="includeAbility"/> is set and this season's numbers when
    /// <paramref name="includeStats"/> is. Only what is asked for is fetched.
    /// </summary>
    Task<Squad> GetSquadAsync(
        IReadOnlyCollection<AttributeDefinition> attributes,
        bool includeAbility,
        bool includeStats,
        CancellationToken cancellationToken = default);
}
